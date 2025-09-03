# fsb2

## Description

fsb2 is a preprocessor and converter of Forth source files. Its goal is
to make it easy to edit and maintain Forth source files, in ordinary
text format, for Forth systems that use blocks.

Following some simple layout conventions, the source file can be edited
without the lack of space imposed by blocks, thus making it possible to
include detailed comments and arrange a clear layout.

fsb2’s web page: <http://programandala.net/en.program.fsb2.html>.

## Origin and motivation

fsb2 was written as a faster and simpler alternative to a previous
project of the author,
[fsb](http://programandala.net/en.program.fsb.html) ([fsb in
GitHub](http://github.com/programandala-net/fsb)), which is written in
VimL.

fsb and fsb2 do the same: they convert the same source format and
provide the same target formats. But there are some differences:

- fsb is much slower.

- fsb recognizes three directives: `#previm`, `#vim` and `#trace`.

- fsb allows comments and code at the right of a backslash-format block
  header.

- fsb removes the source filename extension from the target filename.

- fsb doesn’t support TRD disk images as target format.

fsb and fsb2 can be installed at the same time.

## Requirements

fsb2 is written in Forth for Gforth. Probably the changes required to
run on any other standard Forth system are very simple. This task is on
the to-do list.

- [Gforth](http://www.gnu.org/software/gforth/)

- [Forth Foundation Library](http://irdvo.github.io/ffl/)

- Some additional converters have specific requirements. See their
  source code (the files with the ".sh" filename extension).

fsb2 can run on any platform Gforth can run on, and convert files from
the FSB format to FB or FBS formats (see below). But the other
converters, provided only as shell files, run only on GNU/Linux or other
flavours of Unix.

## Forth source filename extensions

The filename extensions used by fsb2 are derived from ".fs" and ".fb",
used by Gforth and other Forth systems.

<table>
<colgroup>
<col style="width: 50%" />
<col style="width: 50%" />
</colgroup>
<thead>
<tr>
<th style="text-align: left;">Extension</th>
<th style="text-align: left;">Description</th>
</tr>
</thead>
<tbody>
<tr>
<td style="text-align: left;"><p>.fs</p></td>
<td style="text-align: left;"><p>Forth stream file.</p></td>
</tr>
<tr>
<td style="text-align: left;"><p>.fsb</p></td>
<td style="text-align: left;"><p>Forth stream file with blocks-like
layout.</p></td>
</tr>
<tr>
<td style="text-align: left;"><p>.fb</p></td>
<td style="text-align: left;"><p>Forth blocks file.</p></td>
</tr>
<tr>
<td style="text-align: left;"><p>.fbs</p></td>
<td style="text-align: left;"><p>Forth blocks file with stream-like
end-of-lines.</p></td>
</tr>
</tbody>
</table>

## The FBS format

FBS is the name fsb2 uses for a *target format* it can convert FSB
sources to. FBS files are blocks files but with 63-chars lines and end
of lines. This is the format used by the library of the lina Forth
system.

## The FSB format

fsb2 uses an ad-hoc simple format for Forth source, called FSB. The
".fsb" filename extension is used. FSB files are ordinary text files
with only three simple layout conventions: block headers, metacomments
and empty lines.

### Block headers

Block headers must be marked with a comment. There are three ways to do
it:

1.  A paren comment that starts at the first column of the line.

2.  A backslash comment that starts at the first column of the line
    **and** ends with a second closing slash. This alternative is
    provided in order to include words with closing parens.

3.  A dot-paren comment that starts at the first column of the line.

More comments or Forth code can be included after the header comment,
except when using the backslash comment header.

Examples:

    ( Tools)

        \ this is block 0

    .( More tools) \ anything is allowed here

        \ this is block 1

    ( MAIN-WORD ) page

        \ this is block 2

    \ (CORE) \

        \ this is block 3

        \ Note: nothing is allowed
        \ on the header line at the right
        \ of the ending backslash.

The only header comment which is not mandatory is the first one, for
block 0. When there is something (except metacomments, see below) at the
top of the file, it’s suppossed to be the contents of the first block.
Example:

    \ This is block 0,
    \ even without an actual block header.

    : bla  ( -- )  recurse  ;

    ( More )

    \ This is block 1.

### Metacomments

Metacomments are comments that will be removed from the target file.
They are backslash comments that are on their own line and have at least
one space on the left.

    ( block header )

    \ This comment will be preserved.

    variable range \ this comment will be preserved as well

      \ But all these comments
      \ will be removed
      \ from the target file.

### Empty lines

All empty lines are ignored and will be removed during the conversion.

## Command line options

    Usage: fsb2 [ OPTION | INPUT-FILE ] ...

      -?, --help    show this help
          --version show version info
      -v, --verbose activate verbose mode
      -b, --fb      convert to FB format (default)
      -s, --fbs     convert to FBS format
      -l, --lines   set the lines per block (default 16)
      -c, --columns set the columns per line (default 64)
      -d, --debug   activate debugging mode (output to the screen)

## Example

The included file \<test.fsb\> can be used for testing:

    # convert test.fsb to test.fsb.fb:
    fsb2 test.fsb

    # convert test.fsb to test.fsb.fbs:
    fsb2 --fbs test.fsb

## Additional converters

Several additional converters are provided as shell files (with the
".sh" filename extension). They are specific to ZX Spectrum Forth
systems, but may be used as a model for other systems.

fsb2-abersoft  
ZX Spectrum TAP file for the original unfixed Abersoft Forth (one file
called "DISC", with 11 1-KiB screens, but 11263 bytes instead of 11264).

fsb2-abersoft11k  
ZX Spectrum TAP file for Abersoft Forth fixed by the Afera library (one
file called "DISC", with 11 1-KiB screens, 11264 bytes).

fsb2-abersoft16k  
ZX Spectrum TAP file for Abersoft Forth improved by the Afera library
(one file called "DISC", with 16 1-KiB screens).

fsb2-mgt  
ZX Spectrum MGT file (disk image for GDOS, G+DOS or Beta DOS), with the
Forth source saved on the sectors.

fsb2-superforth  
Sinclair QL SuperForth individual block files.

fsb2-tap  
ZX Spectrum TAP file (tape), for any ZX Spectrum Forth.

fsb2-trd  
ZX Spectrum TRD file (disk image for TR-DOS), with the Forth source
saved on the sectors, except track 0, which is used by the DOS to
recognize the disk.

fsb2-dsk  
ZX Spectrum +3/+3e DSK file (disk image for +3DOS), with the Forth
source saved on the sectors, except two of them used by the DOS.

## Installation

1.  Edit \<CONFIG.sh\> and comment/uncomment your options, which are
    explained in the file.

2.  Execute \<INSTALL.sh\>.

You can uninstall the program executing \<UNINSTALL.sh\>.

## Repository

Since 2025-09 the [fsb2’s
repository](https://hg.sr.ht/~programandala_net/fsb2) is powered by
[Mercurial](https://mercurial-scm.org) instead of Git and hosted on
[SourceHut](https://sr.ht/).

For convenience, the [fsb2’s old repository on
GitHub](https://github.com/programandala-net/fsb2), created in 2015-11,
is kept as a mirror, handled by Mercurial’s
[hg-git](https://wiki.mercurial-scm.org/HgGit) extension.
