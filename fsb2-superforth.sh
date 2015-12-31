#!/bin/sh

# fsb2-superforth.sh

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
# to a set of individual block files suitable for Sinclair QL
# SuperForth.

# ##############################################################
# Requirements

# fsb2:
#   <http://programandala.net/en.program.fsb2.html>
#
# mmv, by Vladimir Lanin:
#   Included on most Linux distros.

# ##############################################################
# Usage (after installation)

#   fsb2-superforth filename.fsb

# ##############################################################
# History

# 2015-12-28: Start.
#
# 2015-12-31: Removed the redirection from `mmv`; it was a
# remain of a previous version and caused trouble.

# ##############################################################
# Error checking

if [ "$#" -ne 1 ] ; then
  echo "Convert a Forth source file from .fsb to SuperForth block files"
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

# Split the blocks file into individual files of one block:

split \
  --bytes=1024 \
  --numeric-suffixes \
  --suffix-length=4 \
  $blocksfile BLK

# Rename the block files, remove the leading zeros from the
# numeric suffix:

mmv "BLK*[1-9]*" "BLK#2#3"

# Remove block 0:

rm -f BLK0*

# Remove the blocks file:

rm -f $blocksfile

# vim:tw=64:ts=2:sts=2:et:
