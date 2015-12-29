#!/bin/sh

# UNINSTALL.sh

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

# This program uninstalls fsb2.
#
# Edit <CONFIG.sh> first to suit your system.

# ##############################################################
# Usage

#   UNINSTALL.sh

# ##############################################################
# History

# 2015-10-12: First version.
# 2015-12-29: SuperForth converter.

# ##############################################################

. ./CONFIG.sh

rm -f $BINDIR/fsb2
rm -f $BINDIR/fsb2-abersoft
rm -f $BINDIR/fsb2-abersoft11k
rm -f $BINDIR/fsb2-abersoft16k
rm -f $BINDIR/fsb2-mgt
rm -f $BINDIR/fsb2-superforth
rm -f $BINDIR/fsb2-tap
