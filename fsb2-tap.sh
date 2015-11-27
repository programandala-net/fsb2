#!/bin/sh

# fsb2-tap.sh

# This file is part of fsb2
# http://programandala.net/en.program.fsb2.html

# ##############################################################
# Author and license

# Copyright (C) 2015 Marcos Cruz (programandala.net)

# You may do whatever you want with this work, so long as you
# retain the copyright notice(s) and this license in all
# redistributed copies and derived works. There is no warranty.

# ##############################################################
# Description

# This program converts a Forth source file from the FSB format
# to a ZX Spectrum TAP file.

# ##############################################################
# Requirements

# fsb2:
#   <http://programandala.net/en.program.fsb2.html>
# bin2code:
#   <http://metalbrain.speccy.org/link-eng.htm>.

# ##############################################################
# Usage (after installation)

#   fsb2-tap.sh filename.fsb

# ##############################################################
# History

# 2015-10-12: Adapted from fsb
# (http://programandala.net/en.program.fsb.html).

# ##############################################################
# Error checking

if [ "$#" -ne 1 ] ; then
  echo "Convert a Forth source file from .fsb to .tap format"
  echo 'Usage:'
  echo "  ${0##*/} sourcefile.fsb"
  exit 1
fi

if [ ! -e "$1"  ] ; then
  echo "Error: <$1> does not exist"
  exit 1
fi

if [ ! -f "$1"  ] ; then
  echo "Error: <$1> is not a regular file"
  exit 1
fi

if [ ! -r "$1"  ] ; then
  echo "Error: <$1> can not be read"
  exit 1
fi

if [ ! -s "$1"  ] ; then
  echo "Error: <$1> is empty"
  exit 1
fi

# ##############################################################
# Main

# Create the .fb blocks file from the original .fsb source:

fsb2 $1

# Get the filenames:

basefilename=${1%.*}
blocksfile=$basefilename.fsb.fb
tapefile=$basefilename.tap
spectrumfilename=${basefilename##*/}

# The bin2code converter uses the host system filename as the Spectrum 10-char
# filename in the TAP file header, and it provides no option to change it.
# That's why, as a partial solution, the base filename is used instead:

ln -s $blocksfile $basefilename

bin2code $basefilename $tapefile
echo "\"$tapefile\" created"

# Remove the intermediate file:

rm -f $blocksfile $basefilename

# vim:tw=64:ts=2:sts=2:et:
