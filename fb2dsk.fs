#! /usr/bin/env gforth

\ fb2dsk.fs

\ This file is part of fsb2
\ http://programandala.net/en.program.fsb2.html

: fb2dsk-version ( -- ca len ) s" 1.4.0+201703052253" ;

\ ==============================================================
\ Author and license

\ Copyright (C) 2015,2016,2017 Marcos Cruz (programandala.net)

\ You may do whatever you want with this work, so long as you
\ retain the copyright notice(s) and this license in all
\ redistributed copies and derived works. There is no warranty.

\ ==============================================================
\ Description

\ This program creates a DSK disk image for ZX Spectrum +3 from a
\ Forth blocks file, storing the blocks in the sectors of the disk
\ image.

\ ==============================================================
\ History

\ 2015-11-08: Start.
\
\ 2015-11-09: First working version: a 482 KiB input file is converted
\ to a 778496 B DSK image.
\
\ 2016-08-14: Start integrating the code into fsb2.
\
\ 2017-02-27: Change the code style (no mandatory double spaces around
\ comments or before semicolon anymore).  Don't assume the extension
\ of the source filename is "fb"; update and improve the messages.
\ Factor `run`.
\
\ 2017-03-02: Use a structure to hold the disk specifications.
\ Add the disk specification to sector 0 of track 0.
\
\ 2017-03-05: Trailing empty blocks are filled with blanks, not with
\ zeroes.

\ ==============================================================
\ To-do

\ Make the format configurable: 180 or 720 KiB.

\ ==============================================================
\ Requirements

\ From Galope
\ http://programandala.net/en.program.galope.html

require galope/minus-extension.fs
require galope/c-to-str.fs
\ require galope/tilde-tilde.fs  \ XXX TMP for debugging

\ ==============================================================
\ Config

0
  cfield: ~disk-type
    \ Byte 0: Disk type
    \   0 = Standard PCW range DD SS ST (and +3)
    \   1 = Standard CPC range DD SS ST system format
    \   2 = Standard CPC range DD SS ST data only format
    \   3 = Standard PCW range DD DS DT
    \   All other values reserved
  cfield: ~disk-geometry
    \ Byte 1: Disk geometry
    \   Bits 0..1 Sidedness
    \     0 = Single sided
    \     1 = Double sided (alternating sides)
    \     2 = Double sided (successive sides)
    \   Bits 2...6 Reserved (set to 0)
    \   Bit 7 Double track
  cfield: ~tracks
    \ Byte 2: Number of tracks per side
  cfield: ~sectors/track
    \ Byte 3: Number of sectors per track
  cfield: ~blocks/sector
    \ Byte 4: Log2(sector size) - 7
  cfield: ~reserved-tracks
    \ Byte 5: Number of reserved tracks
  cfield: ~sectors/block
    \ Byte 6: Log2(block size / 128)
  cfield: ~directory-blocks
    \ Byte 7: Number of directory blocks
  cfield: ~gap-length-R/W
    \ Byte 8: Gap length (read/write)
  cfield: ~gap-length-format
    \ Byte 9: Gap length (format)
constant /disk-specification

create 180k-disk-specification ( -- a )
  $00 c, \ Disc type
  $00 c, \ Disc geometry
  $28 c, \ Tracks
  $09 c, \ Sectors
  $02 c, \ Blocks per sector (sector size)
  $01 c, \ Reserved tracks
  $03 c, \ ?Sectors per block
  $02 c, \ ?Directory blocks
  $2A c, \ Gap length (R/W)
  $52 c, \ Gap length (format)

create 720k-disk-specification ( -- a )
  $03 c, \ Disc type
  $81 c, \ Disc geometry
  $50 c, \ Tracks
  $09 c, \ Sectors
  $02 c, \ Blocks per sector (sector size)
  $02 c, \ Reserved tracks
  $04 c, \ ?Sectors per block
  $04 c, \ ?Directory blocks
  $2A c, \ Gap length (R/W)
  $52 c, \ Gap length (format)

720k-disk-specification value disk-specification

: disk-type ( -- n )
  disk-specification ~disk-type c@ ;

: disk-geometry ( -- n )
  disk-specification ~disk-geometry c@ ;

: tracks ( -- n )
  disk-specification ~tracks c@ ;

: sectors/track ( -- n )
  disk-specification ~sectors/track c@ ;

: blocks/sector ( -- n )
  disk-specification ~blocks/sector c@ ;

: /sector ( -- n )
  256 blocks/sector * ;
  \ bytes per sector

: sides ( -- n )
  \ disk-type 3 = abs 1+ ;
  disk-geometry %111 and 0> abs 1+ ;

: 180k ( -- )
  180k-disk-specification to disk-specification ;
  \ XXX TODO -- Not used.

: 720k ( -- )
  720k-disk-specification to disk-specification ;
  \ XXX TODO -- Not used.

256 constant /track-header    \ bytes
  8 constant /sector-header   \ bytes
  \ size of the headers (also called information blocks)

create sector-buffer  /sector allot

\ ==============================================================
\ Files

0 value input-fid
0 value output-fid
  \ file identifiers

: open-input-file ( ca len -- )
  \ 2dup ." Converting " type cr  \ XXX TMP
  r/o open-file throw  to input-fid ;
  \ Open the input file _ca len_.

: create-output-file ( ca len -- )
  \ 2dup ." Creating " type cr  \ XXX TMP
  w/o create-file throw  to output-fid ;
  \ Create the output file _ca len_.

: open-files ( ca len -- )
  2dup open-input-file
       -extension s" .dsk" s+ create-output-file ;
  \ Open the input file _ca len_
  \ and create its correspondent output file.

: close-files ( -- )
  input-fid close-file throw
  output-fid close-file throw ;

: str>dsk ( ca len -- )
  [ false ] [if]  \ XXX TMP -- debugging
    .s cr dump  cr ." Press any key to continue" key drop
  [else]
    output-fid write-file throw
  [then] ;
  \ Write string _ca len_ to the disk image.

: b>dsk ( b -- )
  c>str str>dsk ;
  \ Write the 8-bit number _b_ to the disk image.

: w>dsk ( u -- )
  256 /mod swap b>dsk b>dsk ;
  \ Write the 16-bit number _u_ to the disk image
  \ (little endian, low byte followed by high byte).

: empty-sector-buffer ( -- )
  sector-buffer /sector blank ;

: read-sector ( -- ca len )
  empty-sector-buffer
  sector-buffer dup /sector input-fid read-file throw
  /sector max ;
  \ Read a sector-size data chunk from the input file.  If the
  \ input file is empty, return a string of blanks instead.
  \ This way the disk image will be completed, no matter the
  \ size of the input file.

\ ==============================================================
\ Disk image

: nulls ( len -- ca len )
  empty-sector-buffer sector-buffer swap ;
  \ Return a string _ca len_ filled with zeros.
  \ _len_ is not greater than the sector size.

: nulls>dsk ( len -- )
  nulls str>dsk ;

: sector-data ( -- )
  read-sector str>dsk ;

: /track ( -- n )
  sectors/track /sector * /track-header + ;

: disk-header ( -- )
  s\" MV - CPCEMU Disk-File\r\nDisk-Info\r\n" str>dsk
  s" fb2dsk        " str>dsk    \ name of creator
  tracks b>dsk                  \ numbers of tracks
  sides b>dsk                   \ number of sides
  /track w>dsk                  \ size of a track
  204 nulls>dsk ;               \ unused

: sector-header ( track side sector -- )
  rot b>dsk            \ track
  swap b>dsk           \ side
  1+ b>dsk             \ sector ID
  blocks/sector b>dsk  \ sector size
  0 b>dsk              \ FDC status register 1  \ XXX TODO
  0 b>dsk              \ FDC status register 2  \ XXX TODO
  0 w>dsk ;            \ unused

: (track-header) ( track side -- )
  s\" Track-Info\r\n" str>dsk
  0 w>dsk               \ unused
  0 w>dsk               \ unused
  swap b>dsk            \ track
  b>dsk                 \ side

  1 b>dsk 2 b>dsk
    \ XXX TODO
    \ the documentation reads these bytes are unused,
    \ but the DSK files created by mkp3fs use these values

  blocks/sector b>dsk   \ sector size
  sectors/track b>dsk   \ number of sectors
  $52 b>dsk             \ GAP#3 length (value copied from mkp3fs)
  $E5 b>dsk ;           \ filler byte (value copied from mkp3fs)

: sector-headers ( track side -- )
  sectors/track 0 ?do 2dup i sector-header loop 2drop ;

: >output ( -- d )
  output-fid file-position throw ;

: complete-track-header ( d -- )
  >output 2swap d-
  d>s 256 swap - nulls>dsk ;
  \ Complete the track header with nulls to make it 256-byte long.
  \ d = output file position at the start of the track header

: track-header ( track side -- )
  >output 2>r
  2dup (track-header) sector-headers
  2r> complete-track-header ;

: sectors>dsk ( -- )
  0 ?do sector-data loop ;

: (side0-track0) ( -- )
  disk-specification /disk-specification str>dsk
  /sector /disk-specification - nulls>dsk
  sectors/track 1- sectors>dsk ;

: (side-track) ( -- )
  sectors/track sectors>dsk ;

: side-track ( track side -- )
  2dup track-header
       + if (side-track) else (side0-track0) then ;

: track ( track -- )
  sides 0 ?do dup i side-track loop drop ;

: dsk ( -- )
  disk-header tracks 0 ?do i track loop ;

: fb>dsk ( ca len -- )
  \ 2dup ." Converting " type cr  \ XXX TMP
  open-files dsk close-files ;
  \ Convert the file whose name is _ca len_ to a DSK disk image.

\ ==============================================================
\ Boot

: about ( -- )
  ." fb2dsk" cr
  ." Converter of Forth source blocks files" cr
  ." to DSK disk images for ZX Spectrum +3" cr
  ." Version " fb2dsk-version type cr
  ." This program is part of fsb2" cr
  ." http://programandala.net/en.program.fsb2.html" cr cr
  ." Copyright (C) 2015,2106,2017 Marcos Cruz (programandala.net)" cr cr
  ." Usage:" cr
  ."   fb2dsk input_file [ input_file ... ] " cr
  ." Any number of input files is accepted." cr
  ." Output filenames will be the input filenames" cr
  ." but with the '.dsk' extension instead of the original one." cr ;

: input-files ( -- n )
  argc @ 1- ;
  \ Number of input files in the command line.

: (run) ( n -- )
  0 do i 1+ arg fb>dsk loop ;

: run ( -- )
  input-files ?dup if (run) else about then ;

run bye

\ vim: tw=64
