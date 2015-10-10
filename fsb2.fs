#! /usr/bin/env gforth

\ fsb2

\ Copyright (C) 2015 Marcos Cruz (programandala.net)

\ You may do whatever you want with this work, so long as you
\ retain the copyright notice(s) and this license in all
\ redistributed copies and derived works. There is no warranty.

s" A-00-201510102014" 2constant version

\ --------------------------------------------------------------
\ Requirements

\ From Forth Foundation Library

include ffl/arg.fs  \ argument parser

\ From Galope

s" /COUNTED-STRING" environment? 0= [if]  255  [then]
constant /counted-string

: -leading  ( ca len -- ca' len' )  bl skip  ;

: trim  ( ca len -- ca' len' )  -leading -trailing  ;

: between  ( n n1 n2 -- f )  1+ within  ;

\ --------------------------------------------------------------
\ Variables and constants

variable verbose      \ flag: verbose mode?
variable input-files  \ counter  \ XXX OLD
variable fbs-format   \ flag: FBS format output instead of FB?

variable input-line   \ counter: current valid line of the input file
variable output-line  \ counter: current valid line of the output file
variable screen       \ counter: current screen
variable screen-line  \ counter: current line (0..15) of the screen

16 constant lines/screen
lines/screen 1- constant max-screen-line

\ --------------------------------------------------------------
\ Files

variable input-fid
variable output-fid

: print>stdout  ( -- )  stdout to outfile-id  ;

: print>output  ( -- )  output-fid @ to outfile-id  ;

: +input ( ca len -- )
  \ Open the input file.
  \ ca len = filename
  r/o open-file abort" Error while opening the input file."
  input-fid !  ;

: +output  ( ca len -- )
  \ Set the given filename as the output file for the printing words.
  w/o create-file abort" Error while opening the output file."
  output-fid !  ;

: +files  ( -- )  +output +input  ;

: -input  ( -- )
  \ Close the input file.
  input-fid @ close-file abort" Error while closing the input file."  ;

: -output  ( -- )
  \ Close the output file.
  print>stdout
  output-fid @ close-file abort" Error while closing the output file."  ;

: -files  ( -- )  -input -output  ;

: output-suffix  ( -- ca len )
  fbs-format @ if  s" .fbs"  else  s" .fb"  then  ;

: >output-filename  ( ca1 len1 -- ca2 len2 )
  output-suffix s+  ;

\ --------------------------------------------------------------
\ Errors

: .counter  ( a -- )  @ 4 .r cr  ;

: report  ( -- )
  ." Input line: " input-line   .counter
  ." Output line:" output-line  .counter
  ." Screen:     " screen       .counter
  ." Screen line:" screen-line  .counter  ;

: error  ( ca len -- )  -files cr ." ERROR: " type cr cr report abort  ;

: line-too-long.error  ( ca len -- )  s" Line too long: " 2swap s+ error  ;

: screen-too-long.error  ( -- )  s" Screen too long." error  ;

\ --------------------------------------------------------------
\ Converter

: first-name  ( ca1 len1 -- ca2 len2 )
  \ Return the first name _ca2 len2_ of the string _ca1 len1_.
  bl skip 2dup bl scan nip -  ;

: last-name  ( ca1 len1 -- ca2 len2 )
  \ Return the last name _ca2 len2_ of the string _ca1 len1_.
  trim begin  2dup bl scan bl skip dup  while  2nip  repeat  2drop  ;

: indented?  ( ca len -- f )
  \ Is the given input line indented?
  if  c@ bl =  else  drop false  then  ;

: metacomment?  ( ca len -- f )
  \ Is the given input line a metacomment?
  2dup indented? if     first-name s" \" str=
                 else   2drop false  then  ;

: valid-line?  ( ca len -- f )
  \ Is the given input line a valid line?
  \ A valid line is a non-empty line that is not a metacomment.
  \ 2dup cr '{' emit type '}' emit cr  \ XXX INFORMER
  2dup trim nip if    metacomment? 0=
                else  2drop false  then  ;

2variable possible-screen-marker

: paren-marker?  ( -- f )
  possible-screen-marker 2@ s" (" str=  ;

: dot-paren-marker?  ( -- f )
  possible-screen-marker 2@ s" .(" str=  ;

: paren-screen-marker?  ( ca len -- f )
  \ Is the given input line a screen header with paren marker?
  first-name possible-screen-marker 2!
  paren-marker? ?dup ?exit
  dot-paren-marker? ?dup ?exit
  false  ;

false [if]

: screen-header?  ( ca len -- f )
  \ Is the given input line a screen header?
  2dup indented?  if    2drop false
                  else  paren-screen-marker?  then  ;

[else]

: starting-slash?  ( ca1 len1 -- f )  first-name s" \" str=  ;

: ending-slash?  ( ca1 len1 -- f )  last-name s" \" str=  ;

: slash-screen-marker?  ( ca len -- f )
  \ Is the given input line a screen header with slash marker?
  2dup starting-slash? if    ending-slash?
                       else  2drop false  then  ;

: screen-header?  ( ca len -- f )
  \ Is the given input line a screen header?
  2dup indented?             if  2drop  false exit  then
  2dup slash-screen-marker?  if  2drop   true exit  then
       paren-screen-marker?  ;

[then]

create (empty-line) c/l chars allot

: empty-line  ( ca len -- )  (empty-line) c/l  ;

empty-line blank

: padded  ( ca1 len1 -- ca2 len2 )
  \ Pad a line with spaces.
  empty-line s+ drop c/l fbs-format @ +   ;

: screen-too-long?  ( -- f )  screen-line @ max-screen-line >  ;

: next-screen-line  ( -- )
  1 screen-line +!  screen-too-long? if  screen-line off  then  ;

: line>target  ( ca len -- )
  screen-line @ 2 .r  \ XXX INFORMER
  padded type  fbs-format @ if  cr  then  next-screen-line  ;

: complete-screen  ( -- )
  \ Create empty lines to complete the current screen.
  lines/screen screen-line @ - 0 ?do  empty-line line>target  loop
  0 screen-line !  ;

: missing-screen-lines?  ( -- f )
  screen-line @ 1 max-screen-line between  ;

: screen-header  ( -- )
  \ The current line is a screen header. Start a new screen.
  screen-too-long? if  screen-too-long.error  then
  missing-screen-lines? if  complete-screen  then  ;

: ?length  ( ca len -- )
  \ Abort if _len_ is too big.
  dup [ c/l 1- ] literal > if    line-too-long.error
                           else  2drop  then  ;

: (process-line)  ( ca len -- )
  \ Process a valid line of the input file.
  2dup ?length
  1 output-line +!
  2dup screen-header? if    screen-header  then  line>target  ;

: process-line  ( ca len -- )
  1 input-line +!
  2dup valid-line?  if  (process-line)  else  2drop  then  ;

create line-buffer /counted-string chars allot

: init-counters  ( -- )
  input-line off  output-line off  screen off  screen-line off  ;

: get-line  ( -- ca len f )
  line-buffer dup /counted-string input-fid @ read-line throw  ;

: init-converter  ( -- )
  init-counters  print>output  ;

: converter  ( -- )
  init-converter  begin  get-line  while  process-line  repeat  ;

\ --------------------------------------------------------------
\ Misc

: echo  ( ca len -- )
  verbose @ if  type cr  else  2drop  then  ;

\ --------------------------------------------------------------
\ Argument parser

\ Create a new argument parser
s" fsb2.fs"  \ name
s" [OPTION...] INPUT-FILE"  \ usage
version
s" Written in Forth with Gforth by Marcos Cruz (programandala.net)" \ extra
arg-new constant arguments

\ Add the default options
arguments arg-add-help-option
arguments arg-add-version-option

\ Add the verbose option
4 constant arg.verbose-option
char v  \ short option
s" verbose"  \ long option
s" activate verbose mode"  \ description
true  \ switch type
arg.verbose-option arguments arg-add-option

\ Add the FB option (default)
5 constant arg.fb-option
char b  \ short option
s" fb"  \ long option
s" convert to FB format"  \ description
true  \ switch type
arg.fb-option arguments arg-add-option

\ Add the FBS option
6 constant arg.fbs-option
char s  \ short option
s" fbs"  \ long option
s" convert to FBS format"  \ description
true  \ switch type
arg.fbs-option arguments arg-add-option

variable helped  helped off  \ flag: has the help been shown?

: help  ( -- )
  \ Show the help
  arguments arg-print-help  helped on  ;

: aid  ( -- )
  \ Show the help if necessary; executed before quitting the program
  input-files @ helped @ or 0= if  help  then  ;

: verbose-option  ( -- )
  verbose on s" Verbose mode is on" echo  ;

: ?input-files  ( -- )  \ XXX OLD
  \ XXX TMP
  input-files @ abort" Too many input files."
  1 input-files +!  ;

: input-file  ( ca len -- )
  \ 2dup cr ." input-file = " type  \ XXX INFORMER
  \ ?input-files  \ XXX OLD
  2dup >output-filename +files converter -files  ;

: version-option  ( -- )
  arguments arg-print-version  ;

: option  ( n -- )
  \ dup cr ." option = " . \ XXX INFORMER
  case
    arg.help-option       of  help              endof
    arg.version-option    of  version-option    endof
    arg.non-option        of  input-file        endof
    arg.verbose-option    of  verbose-option    endof
    arg.fb-option         of  fbs-format off    endof
    arg.fbs-option        of  fbs-format on     endof
  endcase  ;

: option?  ( -- n flag )
  \ Parse the next option. Is it right?
  arguments arg-parse  dup arg.done <> over arg.error <> and  ;

\ --------------------------------------------------------------
\ Boot

: run  ( -- )
  argc off  \ make Gforth not process the arguments
  begin  option?  while  option  repeat  drop  aid  ;

run bye

\ --------------------------------------------------------------
\ History

\ 2015-10-07: Start.
\ 2015-10-10: First working version.
