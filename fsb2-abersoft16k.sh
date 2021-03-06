#!/bin/sh

# fsb2-abersoft16k.sh

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
# to a ZX Spectrum TAP file suitable for Abersoft Forth, after
# being extended by Afera library
# (http://programandala.net/en.program.afera.html) to use 16-KiB
# RAM-disk files (ZX Spectrum 128 only).
#
# For the ordinary 11-KiB RAM-disk files, as fixed by Afera, use
# <fsb2-abersoft11k.sh>; for the original system (without Afera)
# use <fsb2-abersoft.sh>.

# ##############################################################
# Requirements

# fsb2:
# 	<http://programandala.net/en.program.fsb2.html>
# bin2code:
#   <http://metalbrain.speccy.org/link-eng.htm>.

# ##############################################################
# Usage (after installation)

#   fsb2-abersoft16k.sh filename.fsb

# ##############################################################
# History

# 2015-10-12: Adapted from fsb
# (http://programandala.net/en.program.fsb.html).

# ##############################################################
# Error checking

if [ "$#" -ne 1 ] ; then
  echo "Convert a Forth source file from .fsb to .tap format for Abersoft Forth(expanded by Afera library to use 16-KiB RAM-disks)"
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

fsb2 $1

# Get the filenames:

basefilename=${1%.*}
blocksfile=$basefilename.fb
tapefile=$basefilename.tap

head --bytes=16384 $blocksfile > DISC
  
# The bin2code converter uses the host system filename in the
# TAP file header (there's no option to change it) and Abersoft
# Forth needs the file in the tape to be called "DISC".  That's
# why an intermediate file called "DISC" is used.

bin2code DISC $tapefile
echo "\"$tapefile\" created"

# Remove the intermediate files:

rm -f DISC $blocksfile
	
# vim:tw=64:ts=2:sts=2:et:
