#! /usr/bin/env gforth

\ fsb2

\ http://programandala.net/en.program.fsb2.html

s" A-00-201510150142" 2constant version

\ --------------------------------------------------------------
\ Author and license

\ Copyright (C) 2015 Marcos Cruz (programandala.net)

\ You may do whatever you want with this work, so long as you retain
\ the copyright notice(s) and this license in all redistributed copies
\ and derived works. There is no warranty.

\ --------------------------------------------------------------
\ History

\ 2015-10-07: Start.
\
\ 2015-10-10: First working version.
\
\ 2015-10-15: Lines and columns are configurable. Not tested yet.

\ --------------------------------------------------------------
\ To-do

\ Remove the suffix of the input file.
\ Option to choose a suffix for the output file.

\ --------------------------------------------------------------
\ Requirements

only forth definitions  decimal
warnings off

\ From Forth Foundation Library
\ (http://irdvo.github.io/ffl/)

include ffl/arg.fs  \ argument parser

\ From Galope
\ (http://programandala.net/en.program.galope.html)

s" /COUNTED-STRING" environment? 0= [if]  255  [then]
constant /counted-string

: -leading  ( ca len -- ca' len' )
  bl skip  ;

: trim  ( ca len -- ca' len' )
  -leading -trailing  ;

: between  ( n n1 n2 -- f )
  1+ within  ;

\ --------------------------------------------------------------
\ Variables

variable verbose      \ flag: verbose mode? \ XXX not used yet
variable fbs-format   \ flag: FBS format output instead of FB?

variable input-line#   \ counter: current line of the input file (1..x)
variable output-line#  \ counter: current line of the output file (0..x)
variable screen#       \ counter: current screen (0..x)
variable screen-line#  \ counter: current screen line (0..15)

variable options       \ counter: valid options

\ --------------------------------------------------------------
\ Configuration

16 value lines/screen
64 value columns/line

: max-screen-line  ( -- n ) lines/screen 1-  ;
: max-screen-char  ( -- n ) columns/line 1-  ;

\ --------------------------------------------------------------
\ Files

variable input-fid
variable output-fid

: print>stdout  ( -- )
  stdout to outfile-id  ;

: print>output  ( -- )
  output-fid @ to outfile-id  ;

: +input ( ca len -- )
  \ Open the input file.
  \ ca len = filename
  r/o open-file abort" Error while opening the input file."
  input-fid !  ;

: +output  ( ca len -- )
  \ Set the given filename as the output file for the printing words.
  w/o create-file abort" Error while opening the output file."
  output-fid !  ;

: +files  ( -- )
  +output +input  ;

: -input  ( -- )
  \ Close the input file.
  input-fid @ close-file abort" Error while closing the input file."  ;

: -output  ( -- )
  \ Close the output file.
  print>stdout
  output-fid @ close-file abort" Error while closing the output file."  ;

: -files  ( -- )
  -input -output  ;

: output-suffix  ( -- ca len )
  fbs-format @ if  s" .fbs"  else  s" .fb"  then  ;

: >output-filename  ( ca1 len1 -- ca2 len2 )
  output-suffix s+  ;

\ --------------------------------------------------------------
\ Errors

: .counter  ( a -- )
  \ Print the contents of the given counter variable.
  @ 4 .r cr  ;

: report  ( -- )
  \ Print the counters.
  ." Input line: " input-line#   .counter
  ." Output line:" output-line#  .counter
  ." Screen:     " screen#       .counter
  ." Screen line:" screen-line#  .counter  ;

: error  ( ca len -- )
  \ Abort with the given error message.
  -files cr ." ERROR: " type cr cr report abort  ;

: line-too-long.error  ( ca len -- )
  s" Line too long: " 2swap s+ error  ;

: screen-too-long.error  ( -- )
  s" Screen too long." error  ;

\ --------------------------------------------------------------
\ Converter

: echo  ( ca len -- )
  verbose @ if    print>stdout type cr  print>output
            else  2drop  then  ;

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

: starting-slash?  ( ca1 len1 -- f )
  first-name s" \" str=  ;

: ending-slash?  ( ca1 len1 -- f )
  last-name s" \" str=  ;

: slash-screen-marker?  ( ca len -- f )
  \ Is the given input line a screen header with slash marker?
  2dup starting-slash? if    ending-slash?
                       else  2drop false  then  ;

: screen-header?  ( ca len -- f )
  \ Is the given input line a screen header?
  2dup indented?             if  2drop  false exit  then
  2dup slash-screen-marker?  if  2drop   true exit  then
       paren-screen-marker?  ;

create (empty-line) /counted-string chars allot

: empty-line  ( ca len -- )
  (empty-line) columns/line  ;

empty-line blank

: padded  ( ca1 len1 -- ca2 len2 )
  \ Pad the given input line to `columns/line` spaces,
  \ or one less if `fbs-format` is on.
  empty-line s+ drop columns/line fbs-format @ +   ;

: screen-too-long?  ( -- f )
  \ Is the current screen too long?
  screen-line# @ max-screen-line >  ;

: next-screen-line  ( -- )
  \ Update the screen line counter.
  1 screen-line# +!  screen-too-long? if  screen-line# off  then  ;

: print-line  ( ca len -- )
  \ Print the given input line to the output file.
  \ screen-line# @ 2 .r  \ XXX INFORMER
  padded type  fbs-format @ if  cr  then  next-screen-line  ;

: missing-screen-lines?  ( -- f )
  screen-line# @ 1 max-screen-line between  ;

: (complete-screen)  ( -- )
  \ Create empty lines to complete the current screen.
  lines/screen screen-line# @ - 0 ?do  empty-line print-line  loop
  screen-line# off  ;

: complete-screen  ( -- )
  \ Complete the current screen, if needed.
  missing-screen-lines? if  (complete-screen)  then  ;

: finish-screen  ( -- )
  \ Check and complete the current screen.
  screen-too-long? if    screen-too-long.error
                   else  complete-screen  then  ;

: check-length  ( ca len -- )
  \ Abort if the length of the given input line is too long.
  dup max-screen-char > if    line-too-long.error
                        else  2drop  then  ;

: (process-line)  ( ca len -- )
  \ Process a valid input line.
  2dup check-length
  1 output-line# +!
  2dup screen-header? if  finish-screen  then  print-line  ;

: process-line  ( ca len -- )
  1 input-line# +!
  2dup valid-line?  if  (process-line)  else  2drop  then  ;

create line-buffer /counted-string chars allot

: init-counters  ( -- )
  input-line# off  output-line# off  screen# off  screen-line# off  ;

: get-line  ( -- ca len f )
  line-buffer dup /counted-string input-fid @ read-line throw  ;

: init-converter  ( -- )
  init-counters  print>output  ;

: converter  ( -- )
  init-converter  begin  get-line  while  process-line  repeat
  complete-screen  ;

\ --------------------------------------------------------------
\ Argument parser

\ Create a new argument parser
s" fsb2"  \ name
s" [ OPTION | INPUT-FILE ] ..."  \ usage
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
s" convert to FB format (default)"  \ description
true  \ switch type
arg.fb-option arguments arg-add-option

\ Add the FBS option
6 constant arg.fbs-option
char s  \ short option
s" fbs"  \ long option
s" convert to FBS format"  \ description
true  \ switch type
arg.fbs-option arguments arg-add-option

\ Add the lines option
7 constant arg.lines-option
char l  \ short option
s" lines"  \ long option
s" set the lines per screen (default 16)"  \ description
false  \ switch type
arg.lines-option arguments arg-add-option

\ Add the columns option
8 constant arg.columns-option
char c  \ short option
s" columns"  \ long option
s" set the columns per line (default 64)"  \ description
false  \ switch type
arg.columns-option arguments arg-add-option

: help  ( -- )
  \ Show the help
  arguments arg-print-help  ;

: aid  ( -- )
  \ Show the help if no option was specified.
  options @ ?exit  help  ;

: verbose-option  ( -- )
  verbose on  s" Verbose mode is on" echo  ;

: fbs-option  ( --  )
  fbs-format on  s" FBS format is on" echo  ;

: fb-option  ( --  )
  fbs-format off  s" FB format is on" echo  ;

: columns-option  ( ca len --  )
  2dup s>number? 0= abort" Wrong columns argument"
  s>d to columns/line
  s"  columns per line" s+ echo  ;

: lines-option  ( ca len --  )
  2dup s>number? 0= abort" Wrong lines argument"
  s>d to lines/screen  
  s"  lines per screen" s+ echo  ;

: input-file  ( ca len -- )
  2dup s" Converting " 2swap s+ echo
  2dup >output-filename +files converter -files  ;

: version-option  ( -- )
  arguments arg-print-version  ;

: option  ( n -- )
  1 options +!
  case
    arg.help-option       of  help            endof
    arg.version-option    of  version-option  endof
    arg.non-option        of  input-file      endof
    arg.verbose-option    of  verbose-option  endof
    arg.fb-option         of  fb-option       endof
    arg.fbs-option        of  fbs-option      endof
    arg.columns-option    of  columns-option  endof
    arg.lines-option      of  lines-option    endof
  endcase  ;

: option?  ( -- n f )
  \ Parse the next option. Is it right?
  arguments arg-parse  dup arg.done <> over arg.error <> and  ;

\ --------------------------------------------------------------
\ Boot

: init  ( -- )
  argc off  \ make Gforth not process the arguments
  verbose off  fbs-format off  options off  ;

: run  ( -- )
  init  begin  option?  while  option  repeat  drop  aid  ;

run bye

