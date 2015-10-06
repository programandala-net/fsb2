#! /usr/bin/env gforth

\ fsb2

\ Copyright (C) 2015 Marcos Cruz (programandala.net)

\ You may do whatever you want with this work, so long as you
\ retain the copyright notice(s) and this license in all
\ redistributed copies and derived works. There is no warranty.

s" A-00-20151007" 2constant version

\ --------------------------------------------------------------
\ Requirements

\ From Forth Foundation library
include ffl/arg.fs  \ argument parser

\ --------------------------------------------------------------
\ Converter

: first-name  ( ca1 len1 -- ca2 len2 )
  \ Return the first name _ca2 len2_ of the string _ca1 len1_.
  \ XXX TODO
  ;

: indented?  ( ca len -- f )
  \ Is the given code line indented?
  0= if  drop false exit  then  c@ bl =  ;

: metacomment?  ( ca len -- f )
  \ Is the given code line a metacomment?
  2dup indented? if     first-name s" \" str=
                 else   2drop false  then  ;

variable line  \ current line of the input file
variable screen  \ current screen of the input file

: valid-line?  ( ca len -- f )
  \ Is the given code line a valid line?
  \ A valid line is a non-empty line that is not a metacomment.
  dup 0= if  2drop false exit  then
  metacomment? 0=  ;

: block-marker?  ( ca len -- f )
  \ Is the given string the a word used as block marker?
  2dup s" ("  str= if  2drop true exit  then
       s" .(" str= if  true exit  then
  false  ;

: block-header?  ( ca len -- f )
  \ Is the given code line a block header?
  2dup indented?  if    false exit
                  else  first-name block-marker?  then
  else
  false  ;

: code-line  ( ca len -- )
  \ Process a valid line.
  ;

: (fsb2)  ( -- )
  line off  screen off
  0.
  begin
    2drop line-buffer over swap file read-line throw
  while
    2dup valid-line? 0=
  while  ( ca len )
    code-line
  repeat then  ;

: fsb2  ( ca len -- )
  \ ca len = input file
  verbose @ if  cr 2dup type  then  (fs2zx)
  verbose @ if  cr  then  ;

\ --------------------------------------------------------------
\ Misc

: echo  ( ca len -- )
  verbose @ if  type cr  else  2drop  then  ;

\ --------------------------------------------------------------
\ Argument parser and boot

\ Create a new argument parser
s" fsb2.fs"  \ name
s" [OPTION | FILE] ... [OPTION | FILE]"  \ usage
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
5 constant arg.fsb-option
char b  \ short option
s" fb"  \ long option
s" convert to FB format"  \ description
true  \ switch type
arg.command-option arguments arg-add-option

\ Add the FBS option
6 constant arg.fbs-option
char s  \ short option
s" fbs"  \ long option
s" convert to FBS format"  \ description
true  \ switch type
arg.command-option arguments arg-add-option

variable helped  helped off  \ flag: has the help been shown?
: help  ( -- )
  arguments arg-print-help  helped on
  ;
: aid  ( -- )
  \ Show the help if necessary; executed before quitting the program
  files @ helped @ or 0= if  help  then
  ;
: verbose-option  ( -- )
  verbose on s" Verbose mode is on" echo
  ;
: input-file  ( ca len -- )
  1 files +!  fs2zx  
  ;
: version-option  ( -- )
  arguments arg-print-version
  ;
: option  ( n -- )
  case
    arg.non-option        of  fsb2              endof
    arg.verbose-option    of  verbose-option    endof
    arg.command-option    of  command-option    endof
    arg.version-option    of  version-option    endof
    arg.help-option       of  help              endof
  endcase
  ;
: option?  ( -- n flag )
  \ Parse the next option. Is it right?
  arguments arg-parse  dup arg.done <> over arg.error <> and
  ;
: (run)  ( -- )
  begin  option?  while  option  repeat  drop
  ;
: run  ( -- )
  (run) aid finish
  ;

run

\ --------------------------------------------------------------
\ History

\ 2015-10-07: Start.
