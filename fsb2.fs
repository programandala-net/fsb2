#! /usr/bin/env gforth

\ fsb2

\ http://programandala.net/en.program.fsb2.html

s" A-00-201510161142" 2constant version

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
\
\ 2015-10-16: Converted to standard code, without the Gforth-specific
\ output redirection.

\ --------------------------------------------------------------
\ To-do

\ XXX FIXME -- the screen too long check does not work fine
\
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

true constant [standard]  immediate
  \ Standard code instead of Gforth specific?

\ --------------------------------------------------------------
\ Files

variable input-fid
variable output-fid

[standard] [if]

: print>terminal  ;
: print>output  ;

[else]

: print>terminal  ( -- )
  stdout to outfile-id  ;

: print>output  ( -- )
  output-fid @ to outfile-id  ;

[then]

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
  print>terminal
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
  verbose @ if    print>terminal type cr  print>output
            else  2drop  then  ;

  \ XXX TODO copy to Galope; used also in Solo Forth
: /name  ( ca1 len1 -- ca2 len2 ca3 len3 )
  \ ca1 len1 = Text.
  \ ca2 len2 = Same text, from the start of its first name.
  \ ca3 len3 = Same text, from the char after its first name.
  bl skip 2dup bl scan  ;

  \ XXX TODO copy to Galope; used also in Solo Forth
: first-name  ( ca1 len1 -- ca2 len2 )  /name nip -  ;
  \ Return the first name _ca2 len2_ of the string _ca1 len1_.

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

: actual-line?  ( ca len -- f )
  \ Is the given input line an actual code line
  \ (a non-empty line that is not a metacomment)?
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

[standard] [if]

\ : >output  ( ca len -- )  output-fid @ write-file throw  ;
: >output  ( ca len -- )  type cr ;

create eol 1 c, 10 c,
: eol$  ( -- ca len )  eol count  ;

: print-line  ( ca len -- )
  \ Print the given input line to the output file.
  padded >output  fbs-format @ if  eol$ >output  then  ;

[else]

: print-line  ( ca len -- )
  \ Print the given input line to the output file.
  \ screen-line# @ 2 .r  \ XXX INFORMER
  padded type  fbs-format @ if  cr  then  ;

[then]

: missing-screen-lines?  ( -- f )
  screen-line# @ 1 max-screen-line between  ;

: complete-screen  ( -- )
  \ Create empty lines to complete the current screen.
  lines/screen screen-line# @ - 0 ?do  empty-line print-line  loop
  screen-line# off  ;

: new-screen  ( -- )
  \ Start a new screen.
  ." ---------- " cr  \ XXX INFORMER
  missing-screen-lines? if  complete-screen  then  ;

: screen-too-long?  ( -- f )
  \ Is the current screen too long?
  screen-line# @ max-screen-line >  ;

: check-screen-length  ( -- )
  \ Check the current screen length.
  screen-too-long? if  screen-too-long.error  then  ;

: check-line-length  ( ca len -- )
  \ Abort if the length of the given input line is too long.
  dup max-screen-char > if    line-too-long.error
                        else  2drop  then  ;

: (process-line)  ( ca len -- )
  \ Process an actual input line.
  2dup check-line-length  
  1 output-line# +!  check-screen-length
  2dup screen-header? if  new-screen  then  print-line  ;

: process-line  ( ca len -- )
  \ Process an input line.
  1 input-line# +!
  2dup actual-line?  if  (process-line)  else  2drop  then  ;

create line-buffer /counted-string chars allot

: init-counters  ( -- )
  input-line# off  output-line# off  screen# off  screen-line# off  ;

: get-line  ( -- ca len f )
  line-buffer dup /counted-string input-fid @ read-line throw  ;

: init-converter  ( -- )
  init-counters  print>output  ;

: converter  ( -- )
  init-converter  begin  get-line  while  process-line  repeat
  new-screen  ;

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

