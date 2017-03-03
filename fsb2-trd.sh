#!/bin/bash

# fsb2-trd.sh

# This file is part of fsb2
# http://programandala.net/en.program.fsb2.html

# Last modified: 201703031610

# ===============================================================
# Author and license

# Copyright (C) 2016,2017 Marcos Cruz (programandala.net)

# You may do whatever you want with this work, so long as you
# retain the copyright notice(s) and this license in all
# redistributed copies and derived works. There is no warranty.

# ===============================================================
# Description

# This program converts a Forth source file from the FSB format to a
# ZX Spectrum phony TRD disk image (suitable for TR-DOS), The disk
# image will contain the source file directly on the sectors, without
# file system, to be directly accessed by a Forth system.  This is the
# format used by the library disk of Solo Forth
# (http://programandala.net/en.program.solo_forth.html).
#
# XXX TODO -- Add track 0 (16 sectors, 4 KiB) at the start, created by
# <make_trd_track_0.fs>, which is part of Solo Forth.

# ===============================================================
# Requirements

# fsb2:
#   <http://programandala.net/en.program.fsb2.html>

# ===============================================================
# Usage (after installation)

#   fsb2-trd filename [disk_label]

# ===============================================================
# History

# 2016-08-03: Start. Adapt from fsb2-mgt.sh.
#
# 2016-08-11: Add sector 0, which is created by a Gforth program
# originally written for Solo Forth
# (http://programandala.net/en.program.solo_forth.html). Without
# sector 0, which contains the disk metadata, the disk image is not
# recognized by the emulator and can not be mounted.
#
# 2017-02-27: Don't assume the extension of the source filename
# is "fsb" anymore. Don't reuse it as secondary extension of the
# blocks file. Update the messages.
#
# 2017-03-03: Improve the error message about maximum capacity.

# ===============================================================
# Error checking

if [[ "$#" -ne 1 && "$#" -ne 2 ]] ; then
  echo "Convert a Forth source file from FSB format"
  echo "to a block disk in a TRD disk image."
  echo 'Usage:'
  echo "  ${0##*/} sourcefile [disk_label]"
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

# ===============================================================
# Main

# Get the first 8 characters of the disk label, padded with spaces:
disk_label="${2}        "
disk_label=${2:0:8}

# Convert the .fsb file to .fb:
fsb2 $1

# Filenames:
basefilename=${1%.*}
blocksfile=$basefilename.fb
trdfile=$basefilename.trd
track0file="/tmp/fsb2-trd.track_0.bin"
tracks1to79file="/tmp/fsb2-trd.tracks_1-79.bin"

# Get the size of the file:
du_size=$(du -sk $blocksfile)

# Extract the size from the left of the string:
file_size=${du_size%%[^0-9]*}

#echo "File size=($file_size)"
#echo "$blocksfile is $file_size Kib"

if [ $file_size -gt "636" ]
then
  echo "Error:"
  echo "The size of $blocksfile is $file_size KiB."
  echo "The maximum capacity usable for blocks on a TRD disk image"
  echo "is 636 KiB (640 KiB minus the first track)."
  exit 64
fi

# Create sector 0 of the disk image:
fsb2-trd.track_0.fs $track0file $disk_label

# Create sectors 1..79 of the disk image:
dd if=$blocksfile of=$tracks1to79file bs=651264 cbs=651264 conv=block,sync 2>> /dev/null

# Create the final disk image:
cat $track0file $tracks1to79file > $trdfile

# Remove the temporary files:
rm -f $blocksfile $track0file $tracks1to79file

# vim:tw=64:ts=2:sts=2:et:

