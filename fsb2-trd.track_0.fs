#! /usr/bin/env gforth

\ fsb2-trd.track_0.fs

\ This file is part of fsb2
\ http://programandala.net/en.program.fsb2.html

\ Last modified: 201608111205

\ ==============================================================
\ Author and license

\ Copyright (C) 2016 Marcos Cruz (programandala.net)

\ You may do whatever you want with this work, so long as you
\ retain the copyright notice(s) and this license in all
\ redistributed copies and derived works. There is no warranty.

\ ==============================================================
\ Description

\ Create a file with the contents of track 0 of an empty TRD disk
\ image.
\
\ Usage:
\
\   fsb2-trd.track_0.fs output_file.bin [disk_label]

\ ==============================================================
\ History

\ 2016-08-04: Write for the Makefile of Solo Forth
\ (http://programandala.net/en.program.solo_forth.html)
\
\ 2016-08-05: Add a fourth track 0 file. Rename output files after the
\ changes in Makefile. Add disk9 track, for the disk9 debugging disk
\ image.
\
\ 2016-08-11: Add command line arguments and integrate into fsb2.

\ ==============================================================

256 constant /sector
  \ Bytes per sector.

: new-file  ( -- fid )  w/o create-file throw  ;
  \ Create the file and return its file identifier _fid_.

: allocated  ( len -- ca len )  chars dup allocate throw swap  ;
  \ Return an allocated space _ca len_ of _len_ chars.

: >nulls$  ( len -- ca len )  allocated 2dup erase  ;
  \ Return a string _ca len_ of _len_ zeros.

: >spaces$  ( len -- ca len )  allocated 2dup blank  ;
  \ Return a string _ca len_ of _len_ zeros.

: str>file  ( ca len fid -- )  write-file throw  ;
  \ Write string _ca len_ to file _fid_.

: 8b>str  ( 8b -- ca len )  pad c! pad 1  ;
  \ Convert an 8-bit number _8b_ to 1-char string _ca len_.

: 8b>file  ( 8b fid -- )  >r 8b>str r> str>file  ;
  \ Write an 8-bit number _8b_ to file _fid_.

: 16b>file  ( 16b fid -- )
  >r dup $100 mod r@ 8b>file $100 / r> 8b>file  ;
  \ Write a 16-bit number _16b_ to file _fid_, in Z80 format (LSB
  \ first).

8 constant /disk-label
  \ Max characters per disk label.

create disk-label /disk-label chars allot
  \ Create a buffer for the disk-label string.

: disk-label$  ( -- ca len )  disk-label /disk-label  ;
  \ Return the disk-label string.

: >disk-label$  ( ca1 len1 -- ca2 len2 )
  disk-label$  2dup blank  rot max chars move  disk-label$ ;
  \ Return disk label _ca1 len1_, padded with spaces to its maximum
  \ length.

: sector-8  ( ca len fid -- )
  >r
  0 r@ 8b>file                  \ end of directory
  224 >nulls$ r@ str>file       \ unused
  0 r@ 8b>file                  \ first free sector of first free track
  1 r@ 8b>file                  \ first free track
  $16 r@ 8b>file                \ disk type: 80 tracks, double sided
  0 r@ 8b>file                  \ number of files on disk
  $09F0 r@ 16b>file             \ number of free sectors
  $10 r@ 8b>file                \ TR-DOS id byte
  2 >nulls$ r@ str>file         \ unused
  9 >spaces$ r@ str>file        \ unused
  0 r@ 8b>file                  \ unused
  0 r@ 8b>file                  \ number of deleted files
  >disk-label$ r@ str>file      \ 8-char disk label
  3 >nulls$ r> str>file         \ unused
  ;
  \ Save to file _fid_ the contents of sector 8 of track 0 of an empty
  \ TRD disk image with disk label _ca len_.

: empty-sectors  ( n fid -- )  swap /sector * >nulls$ rot str>file  ;
  \ Write _n_ empty sectors to file _fid_.

: fill-track  ( ca len fid -- )
  8 over empty-sectors  dup >r sector-8  7 r> empty-sectors  ;
  \ Fill file _fid_ with the track 0 of an empty TRD disk image,
  \ with disk name _ca len_.

: make-track  ( ca1 len1 ca2 len2 -- )
  new-file dup >r fill-track r> close-file throw  ;
  \ Create file _ca2 len2_ with the contents of track 0 of an empty
  \ TRD disk image with disk name _ca1 len1_.

: arguments  ( -- ca1 len1 ca2 len2 )
  argc @ 3 > abort" Only two arguments allowed"
  argc @ 3 < abort" Two arguments expected"
  2 arg  1 arg  ;
  \ Return command line arguments 2 _ca1 len1_ (disk name) and 1 _ca2
  \ len2_ (track 0 file to create).

arguments make-track  bye
