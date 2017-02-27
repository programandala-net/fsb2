#! /usr/bin/env gforth

\ fsb2
\ A Forth source converter.
\ http://programandala.net/en.program.fsb2.html

\ Last modified: 201702272035

include ./fsb2_VERSION.fs

\ ==============================================================
\ Author and license

\ Copyright (C) 2015,2016,2017 Marcos Cruz (programandala.net)

\ You may do whatever you want with this work, so long as you retain
\ the copyright notice(s) and this license in all redistributed copies
\ and derived works. There is no warranty.

\ ===============================================================
\ Description

\ See <README.adoc>.

\ ===============================================================
\ History

\ See at the end of the file.

\ ===============================================================
\ To-do

\ Option to choose a suffix for the output file.
\
\ Substitute any Gforth-specific code with standard Forth.
\
\ Add the name of the file to the "block too long" error message.

\ ===============================================================
\ Requirements

 only forth definitions  decimal  warnings off

\ From Forth Foundation Library
\ (http://irdvo.github.io/ffl/):

include ffl/arg.fs \ argument parser

\ From Galope
\ (http://programandala.net/en.program.galope.html):

s" /COUNTED-STRING" environment? 0= [if]  255  [then]
constant /counted-string

: -leading ( ca len -- ca' len' )
  bl skip ;

: trim ( ca len -- ca' len' )
  -leading -trailing ;

: between ( n n1 n2 -- f )
  1+ within ;

: /name ( ca1 len1 -- ca2 len2 ca3 len3 )
  bl skip 2dup bl scan ;
  \ Divide a string at its first name (a substring separated by
  \ spaces).
  \ ca1 len1 = Text.
  \ ca2 len2 = Same text, from the start of its first name.
  \ ca3 len3 = Same text, from the char after its first name.

: first-name ( ca1 len1 -- ca2 len2 )
  /name nip - ;
  \ Return the first name _ca2 len2_ (a substring separated by spaces)
  \ of string _ca1 len1_.

: last-name ( ca1 len1 -- ca2 len2 )
  trim begin  2dup bl scan bl skip dup  while  2nip  repeat  2drop ;
  \ Return the last name _ca2 len2_ (a substring separated by spaces)
  \ of string _ca1 len1_.

: -bounds  ( ca1 len -- ca2 ca1 )
  2dup + 1- nip ; \ This works with '-1 +loop'
  \ Convert an address and length to the parameters needed by a
  \ "do ... -1 +loop" in order to examine that memory zone in
  \ reverse order.
  \
  \ XXX Note: "do ... 1 -loop" does not work the same way in Gforth
  \ and can not be used with this '-bounds'.
  \
  \ over 1- >r + 1- r> swap ; \ This variant works with '1 -loop'

: -extension  ( ca1 len1 -- ca1 len1' | ca1 len1 )
  2dup -bounds 1+ 2swap  \ default raw return values
  -bounds ?do
    i c@ '.' = if  drop i  leave  then
  -1 +loop  ( ca1 ca1' )  \ final raw return values
  over - ;
  \ Remove the file extension from a filename.

\ ===============================================================
\ Variables

variable verbose      \ flag: verbose mode? \ XXX not used yet
variable fbs-format   \ flag: FBS format output instead of FB?

variable input-line#          \ current line of the input file (1..x)
variable actual-input-line#   \ current actual line of the input file (1..x)
variable block#               \ current block (0..x)
variable block-line#          \ current block line (0..15)

variable options      \ counter: valid options on the command line

\ ===============================================================
\ Configuration

l/s value lines/block
c/l value columns/line

: max-block-line ( -- n ) lines/block 1- ;
: max-block-column ( -- n ) columns/line 1- ;

\ ===============================================================
\ Files

variable input-fid
variable output-fid

: +input ( ca len -- )
  r/o open-file abort" Error while opening the input file."
  input-fid ! ;
  \ Open the input file whose filename is _ca len_.

: +output ( ca len -- )
  w/o create-file abort" Error while opening the output file."
  output-fid ! ;
  \ Set the given filename as the output file for the printing words.

: +files ( ca1 len1 ca2 len2 -- )
  +output +input ;
  \ Set the filename _ca2 len2_ as the output file for the printing
  \ words; open the input file _ca1 len1_.

: -input ( -- )
  input-fid @ close-file abort" Error while closing the input file." ;
  \ Close the input file.

: -output ( -- )
  output-fid @ close-file abort" Error while closing the output file." ;
  \ Close the output file.

: -files ( -- )
  -input -output ;
  \ Close both the input and output files.

: output-extension ( -- ca len )
  fbs-format @ if s" .fbs" else s" .fb" then ;
  \ Return the filename suffix for the current output format.

: >output-filename ( ca1 len1 -- ca2 len2 )
  -extension output-extension s+ ;
  \ Convert the input filename _ca1 len1_
  \ to the output filename _ca2 len2_.

\ ===============================================================
\ Errors

: .counter ( a -- )
  @ 4 .r cr ;
  \ Print the contents of the given counter variable.

: report ( -- )
  ." Input line: " input-line#  .counter
  ." Block:      " block#       .counter
  ." Block line: " block-line#  .counter ;
  \ Print the counters.

: error ( ca len -- )
  -files cr ." ERROR: " type cr cr report abort ;
  \ Abort with the given error message.

: line-too-long.error ( ca len -- )
  s" Line too long: " 2swap s+ error ;

: block-too-long.error ( -- )
  s" Block too long." error ;

\ ===============================================================
\ Converter

: echo ( ca len -- )
  verbose @ if type cr else 2drop then ;
  \ Echo the given string, if verbose mode is on.

: indented? ( ca len -- f )
  if c@ bl = else drop false then ;
  \ Is the given input line indented?

: metacomment? ( ca len -- f )
  2dup indented? if   first-name s" \" str=
                 else 2drop false then ;
  \ Is the given input line a metacomment?

: actual-line? ( ca len -- f )
  2dup trim nip if   metacomment? 0=
                else 2drop false then ;
  \ Is the given input line an actual code line
  \ (a non-empty line that is not a metacomment)?

2variable possible-block-marker

: paren-marker? ( -- f )
  possible-block-marker 2@ s" (" str= ;

: dot-paren-marker? ( -- f )
  possible-block-marker 2@ s" .(" str= ;

: paren-block-marker? ( ca len -- f )
  first-name possible-block-marker 2!
  paren-marker? ?dup ?exit
  dot-paren-marker? ?dup ?exit
  false ;
  \ Is the given input line a block header with paren marker?

: starting-slash? ( ca len -- f )
  first-name s" \" str= ;
  \ Does the given string starts with a trailing slash?

: ending-slash? ( ca len -- f )
  last-name s" \" str= ;
  \ Does the given string ends with a trailing slash?

: slash-block-marker? ( ca len -- f )
  2dup starting-slash? if   ending-slash?
                       else 2drop false then ;
  \ Is the given input line a block header with slash marker?

: block-header? ( ca len -- f )
  2dup indented?           if 2drop false exit then
  2dup slash-block-marker? if 2drop  true exit then
       paren-block-marker? ;
  \ Is the given input line a block header?

create (empty-line) /counted-string chars allot

: empty-line ( ca len -- )
  (empty-line) columns/line ;

empty-line blank

: padded ( ca1 len1 -- ca2 len2 )
  empty-line s+ drop columns/line fbs-format @ +   ;
  \ Pad the given input line to `columns/line` spaces,
  \ or one less if `fbs-format` is on.

: >output ( ca len -- ) output-fid @ write-file throw ;

create eol 1 c, 10 c,
: eol$ ( -- ca len ) eol count ;

defer print-line ( ca len -- )
  \ Print the given input line.

: line>output ( ca len -- )
  padded >output fbs-format @ if eol$ >output then ;
  \ Print the given input line to the output file.

' line>output is print-line

: line>display ( ca len -- )
  block# @ 3 .r  block-line# @ 3 .r  ."  | " type cr ;
  \ Print the given input line to the display, for debugging.

: new-line ( ca len -- )
  print-line 1 block-line# +! ;

: missing-block-lines? ( -- f )
  block-line# @ 1 max-block-line between ;

: (complete-block) ( -- )
  lines/block block-line# @ - 0 ?do empty-line new-line loop ;
  \ Complete the current block with empty lines.

: complete-block ( -- )
  missing-block-lines? if  (complete-block)  then ;
  \ Complete the current block with empty lines, if needed.

: update-block# ( -- )
  actual-input-line# @ 1 <> abs block# +! ;
  \ Update the number of the current block.
  \ The calculation is needed because the first block
  \ (number 0) may start with a block header line
  \ or an ordinary line.

: new-block ( -- )
  complete-block  update-block#  block-line# off ;
  \ Start a new block.

: block-too-long? ( -- f )
  block-line# @ max-block-line > ;
  \ Is the current block too long?

: check-block-length ( -- )
  block-too-long? if  block-too-long.error  then ;
  \ Check the current block length.

: check-line-length ( ca len -- )
  dup max-block-column > if   line-too-long.error
                         else 2drop  then ;
  \ Abort if the length of the given input line is too long.

: (process-line) ( ca len -- )
  1 actual-input-line# +!
  2dup check-line-length
  2dup block-header? if   new-block
                     else check-block-length
                     then new-line ;
  \ Process an actual input line.

: process-line ( ca len -- )
  1 input-line# +!
  2dup actual-line? if (process-line) else 2drop then ;
  \ Process an input line.

create line-buffer /counted-string chars allot

: get-line ( -- ca len f )
  line-buffer dup /counted-string input-fid @ read-line throw ;

: init-converter ( -- )
  input-line# off  actual-input-line# off
  block# off  block-line# off ;

: converter ( -- )
  init-converter  begin  get-line  while  process-line  repeat
  complete-block ;

\ ===============================================================
\ Argument parser

\ Create a new argument parser:

s" fsb2" \ name
s" [ OPTION | INPUT-FILE ] ..." \ usage
fsb2-version
s" Written in Forth with Gforth by Marcos Cruz (programandala.net)" \ extra
arg-new constant arguments

\ Add the default options:

arguments arg-add-help-option
arguments arg-add-version-option

\ Add the verbose option:

4 constant arg.verbose-option
char v \ short option
s" verbose" \ long option
s" activate verbose mode" \ description
true \ switch type
arg.verbose-option arguments arg-add-option

\ Add the FB option (default):

5 constant arg.fb-option
char b \ short option
s" fb" \ long option
s" convert to FB format (default)" \ description
true \ switch type
arg.fb-option arguments arg-add-option

\ Add the FBS option:

6 constant arg.fbs-option
char s \ short option
s" fbs" \ long option
s" convert to FBS format" \ description
true \ switch type
arg.fbs-option arguments arg-add-option

\ Add the lines option:

7 constant arg.lines-option
char l \ short option
s" lines" \ long option
s" set the lines per block (default 16)" \ description
false \ switch type
arg.lines-option arguments arg-add-option

\ Add the columns option:

8 constant arg.columns-option
char c \ short option
s" columns" \ long option
s" set the columns per line (default 64)" \ description
false \ switch type
arg.columns-option arguments arg-add-option

\ Add the debug option:

9 constant arg.debug-option
char d \ short option
s" debug" \ long option
s" activate debugging mode (output to the screen)" \ description
true \ switch type
arg.debug-option arguments arg-add-option

: help ( -- )
  arguments arg-print-help ;
  \ Show the help

: aid ( -- )
  options @ ?exit  help ;
  \ Show the help if no option was specified.

: verbose-option ( -- )
  verbose on  s" Verbose mode is on" echo ;
  \ Turn the verbose mode on.

: debug ( -- )
  ['] line>display is print-line ;
  \ Turn the debug mode on.

: debug-option ( -- )
  debug s" debug mode is on" echo ;
  \ Turn the debug mode on and inform.

: fbs-option ( --  )
  fbs-format on  s" FBS format is on" echo ;
  \ Set the FBS output format.

: fb-option ( --  )
  fbs-format off  s" FB format is on" echo ;
  \ Set the FB output format.

: columns-option ( ca len --  )
  2dup s>number? 0= abort" Wrong columns argument"
  s>d to columns/line
  s"  columns per line" s+ echo ;
  \ Set the columns per line.

: lines-option ( ca len --  )
  2dup s>number? 0= abort" Wrong lines argument"
  s>d to lines/block
  s"  lines per block" s+ echo ;
  \ Set the lines per block.

: input-file ( ca len -- )
  s" Converting " 2over s+ echo
  2dup >output-filename +files converter -files ;
  \ Set the current input file.

: version-option ( -- )
  arguments arg-print-version ;
  \ Show the version.

: option ( n -- )
  1 options +!
  case
    arg.help-option    of  help           endof
    arg.version-option of  version-option endof
    arg.verbose-option of  verbose-option endof
    arg.fb-option      of  fb-option      endof
    arg.fbs-option     of  fbs-option     endof
    arg.columns-option of  columns-option endof
    arg.lines-option   of  lines-option   endof
    arg.debug-option   of  debug-option   endof
    arg.non-option     of  input-file     endof
  endcase ;

: option? ( -- n f )
  arguments arg-parse  dup arg.done <> over arg.error <> and ;
  \ Parse the next option. Is it right?

\ ===============================================================
\ Boot

: init ( -- )
  argc off \ make Gforth not process the arguments
  verbose off  fbs-format off  options off ;

: go ( -- )
  init begin option? while option repeat drop aid ;

go bye

\ ===============================================================
\ History

\ 2015-10-07: Start.
\
\ 2015-10-10: First working version.
\
\ 2015-10-15: Lines and columns are configurable. Not tested yet.
\
\ 2015-10-16: Substituted the Gforth-specific output redirection with
\ standard file words. Fixed the block length check. Improved the
\ rendering of the first block: no header needed anymore. Simplified
\ some parts. Added debug option.
\
\ 2015-11-21: Removed a Gforth-specific detail: the way `version` was
\ stored.
\
\ 2015-11-26: Small simplification.
\
\ 2015-11-27: Unified the format of comments.
\
\ 2016-05-13: Modified some comments.
\
\ 2016-08-03: Change the version number to Semantic Versioning
\ (http://semver.org) and move it to its own file <fsb2_VERSION.fs>.
\
\ 2016-12-21: Update to-do.
\
\ 2017-02-27: Change the code style (no mandatory double spaces around
\ comments or before semicolon anymore).  Remove the filename
\ extension of the input file, don't keep it as secondary extension of
\ the output file.
