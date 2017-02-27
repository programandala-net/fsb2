#! /usr/bin/env gforth

\ fb2dsk.fs

\ This file is part of fsb2
\ http://programandala.net/en.program.fsb2.html

: fb2dsk-version  ( -- ca len )  s" 1.0.1+201608141456"  ;

\ Last modified: 201608141215

\ ==============================================================
\ Author and license

\ Copyright (C) 2015,2016 Marcos Cruz (programandala.net)

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
\ 2015-11-09: First working version: a 482 KiB input file is
\ converted to a 778496 B DSK image.
\
\ 2016-08-14: Start integrating the code into fsb2.

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

  2 value sides
 80 value tracks
  9 value sectors/track
  2 value blocks/sector  \ 256-byte blocks per sector
  \ disk geometry

: /sector  ( -- n )
  256 blocks/sector *  ;
  \ bytes per sector

: 180k  ( -- )
  1 to sides  40 to tracks  ;

: 720k  ( -- )
  2 to sides  80 to tracks  ;

256 constant /track-header    \ bytes
  8 constant /sector-header   \ bytes
  \ size of the headers (also called information blocks)

create sector-buffer  /sector allot

\ ==============================================================
\ Files

0 value input-fid
0 value output-fid
  \ file identifiers

: open-input-file  ( ca len -- )
  2dup ." Converting " type cr  \ XXX TMP
  r/o open-file throw  to input-fid  ;
  \ Open the input file _ca len_.

: create-output-file  ( ca len -- )
  2dup ." Creating " type cr  \ XXX TMP
  w/o create-file throw  to output-fid  ;
  \ Create the output file _ca len_.

: open-files  ( ca len -- )
  2dup open-input-file
       -extension s" .dsk" s+ create-output-file  ;
  \ Open the input file _ca len_
  \ and create its correspondent output file.

: close-files  ( -- )
  input-fid close-file throw
  output-fid close-file throw  ;

: str>dsk  ( ca len -- )
  [ false ] [if]  \ XXX TMP -- debugging
    .s cr dump  cr ." Press any key to continue" key drop
  [else]
    output-fid write-file throw
  [then]  ;
  \ Write string _ca len_ to the disk image.

: b>dsk  ( b -- )
  c>str str>dsk  ;
  \ Write the 8-bit number _b_ to the disk image.

: w>dsk  ( u -- )
  256 /mod swap b>dsk b>dsk  ;
  \ Write the 16-bit number _u_ to the disk image
  \ (little endian, low byte followed by high byte).

: empty-sector-buffer  ( -- )
  sector-buffer /sector erase  ;

: read-sector  ( -- ca len )
  empty-sector-buffer
  sector-buffer dup /sector input-fid read-file throw
  /sector max  ;
  \ Read a sector-size data chunk from the input file.  If the
  \ input file is empty, return a string of zeroes instead.
  \ This way the disk image will be completed, no matter the
  \ size of the input file.

\ ==============================================================
\ Disk image

: nulls  ( len -- ca len )
  empty-sector-buffer sector-buffer swap  ;
  \ return a string _ca len_ filled with zeros.

: sector-data  ( -- )
  read-sector str>dsk  ;

: /track  ( -- n )
  sectors/track /sector * /track-header +  ;

: disk-header  ( -- )
  s\" MV - CPCEMU Disk-File\r\nDisk-Info\r\n" str>dsk
  s" fb2dsk        " str>dsk    \ name of creator
  tracks b>dsk                  \ numbers of tracks
  sides b>dsk                   \ number of sides
  /track w>dsk                  \ size of a track
  204 nulls str>dsk             \ unused
  ;

: sector-header  ( track side sector -- )
  rot b>dsk            \ track
  swap b>dsk           \ side
  1+ b>dsk             \ sector ID
  blocks/sector b>dsk  \ sector size
  0 b>dsk              \ FDC status register 1  \ XXX TODO
  0 b>dsk              \ FDC status register 2  \ XXX TODO
  0 w>dsk              \ unused
  ;

: (track-header)  ( track side -- )
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
  $E5 b>dsk             \ filler byte (value copied from mkp3fs)
  ;

: sector-headers  ( track side -- )
  sectors/track 0 ?do  2dup i sector-header  loop  2drop  ;

: >output  ( -- d )
  output-fid file-position throw  ;

: complete-track-header ( d -- )
  >output 2swap d-
  d>s 256 swap - nulls str>dsk  ;
  \ Complete the track header with nulls to make it 256-byte long.
  \ d = output file position at the start of the track header

: track-header  ( track side -- )
  >output 2>r
  2dup (track-header) sector-headers
  2r> complete-track-header  ;

: (track)  ( track side -- )
  track-header
  sectors/track 0 ?do  sector-data  loop  ;

: track  ( track -- )
  sides 0 ?do  dup i (track)  loop  drop  ;

: dsk  ( -- )
  disk-header  tracks 0 ?do  i track  loop  ;

: fb>dsk  ( ca len -- )
  2dup ." Converting " type cr  \ XXX TMP
  open-files dsk close-files  ;
  \ Convert the file whose name is _ca len_ to a DSK disk image.

\ ==============================================================
\ Boot

: about  ( -- )
  ." fb2dsk" cr
  ." Converter of Forth source blocks files" cr
  ." to DSK disk images for ZX Spectrum +3" cr
  ." Version " fb2dsk-version type cr
  ." This program is part of fsb2" cr
  ." http://programandala.net/en.program.fsb2.html" cr cr
  ." Copyright (C) 2015,2106 Marcos Cruz (programandala.net)" cr cr
  ." Usage (depending on the installation method):" cr
  ."   fb2dsk[.fs] input_file.fb" cr
  ." Any number of input files is accepted." cr
  ." Output file names will be the input file names" cr
  ." but with the '.dsk' extension instead of '.fb'." cr  ;

: input-files  ( -- n )
  argc @ 1- ;
  \ Number of input files in the command line.

: run  ( -- )
  input-files ?dup
  if    0 do  i 1+ arg fb>dsk  loop
  else  about  then  ;

run bye

\ vim: tw=64
