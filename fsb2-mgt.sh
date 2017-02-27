#!/bin/bash

# fsb2-mgt.sh

# This file is part of fsb2
# http://programandala.net/en.program.fsb2.html

# Last modified: 201702271853

# ==============================================================
# Author and license

# Copyright (C) 2015,2016,2017 Marcos Cruz (programandala.net)

# You may do whatever you want with this work, so long as you
# retain the copyright notice(s) and this license in all
# redistributed copies and derived works. There is no warranty.

# ==============================================================
# Description

# This program converts a Forth source file from the FSB format
# to a ZX Spectrum phony MGT disk image (suitable for GDOS,
# G+DOS or Beta DOS). The disk image will contain the source
# file directly on the sectors, without file system, to be
# directly accessed by a Forth system.  This is the format used
# by the library disk of Solo Forth
# (http://programandala.net/en.program.solo_forth.html).

# ==============================================================
# Requirements

# fsb2:
#   <http://programandala.net/en.program.fsb2.html>

# ==============================================================
# Usage (after installation)

#   fsb2-mgt filename

# ==============================================================
# History

# 2015-10-10: Adapted from fsb
# (http://programandala.net/en.program.fsb.html).
#
# 2015-11-21: Typo.
#
# 2016-05-02: Start implementing the size check.
#
# 2016-05-03: Finish the size check.
#
# 2016-08-03: Fix typo.
#
# 2017-02-27: Don't assume the extension of the source filename
# is "fsb" anymore. Don't reuse it as secondary extension of the
# blocks file. Update the messages.

# ==============================================================
# Error checking

if [ "$#" -ne 1 ] ; then
  echo "Convert a Forth source file from FSB format"
  echo "to a block disk in a MGT disk image."
  echo
  echo 'Usage:'
  echo "  ${0##*/} sourcefile"
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

# ==============================================================
# Main

fsb2 --verbose $1

# Get the filenames:

basefilename=${1%.*}
blocksfile=$basefilename.fb
mgtfile=$basefilename.mgt

# Get the size of the file:
du_size=$(du -sk $blocksfile)

# Extract the size from the left of the string:
file_size=${du_size%%[^0-9]*}

#echo "File size=($file_size)"
#echo "$blocksfile is $file_size Kib"

if [ $file_size -gt "800" ]
then
  echo "Error:"
  echo "The size of the intermediate blocks file $blocksfile"
  echo "is $file_size KiB."
  echo "The maximum capacity of an MGT disk image is 800 KiB."
  exit 64
fi

# Do it:

dd if=$blocksfile of=$mgtfile bs=819200 cbs=819200 conv=block,sync 2>> /dev/null

# Remove the temporary file:

rm -f $blocksfile

# vim:tw=64:ts=2:sts=2:et:
