#!/bin/sh

# fsb2-mgt.sh

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
# to a ZX Spectrum phony MGT disk image (suitable for GDOS,
# G+DOS or Beta DOS). The disk image will contain the source
# file directly on the sectors, without file system, to be
# directly accessed by a Forth system.  This is the format used
# by the library disk of Solo Forth
# (http://programanadala.net/en.program.solo_forth.html).

# ##############################################################
# Requirements

# fsb2:
#   <http://programandala.net/en.program.fsb2.html>

# ##############################################################
# Usage (after installation)

#   fsb2-mgt filename.fsb

# ##############################################################
# History

# 2015-10-10: Adapted from fsb
# (http://programandala.net/en.program.fsb.html).
# 2015-11-21: Typo.

# ##############################################################
# Error checking

if [ "$#" -ne 1 ] ; then
  echo "Convert a Forth source file from .fsb to .mgt"
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
blocksfile=$basefilename.fsb.fb
mgtfile=$basefilename.mgt

# Do it:

dd if=$blocksfile of=$mgtfile bs=819200 cbs=819200 conv=block,sync 2>> /dev/null
  
# Remove the temporary file:

rm -f $blocksfile

# vim:tw=64:ts=2:sts=2:et:
