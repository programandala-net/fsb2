#!/bin/sh

# INSTALL.sh

# This file is part of fsb2-
# http://programandala.net/en.program.fsb2-.html

# ##############################################################
# Author and license

# Copyright (C) 2015 Marcos Cruz (programandala.net)

# You may do whatever you want with this work, so long as you
# retain the copyright notice(s) and this license in all
# redistributed copies and derived works. There is no warranty.

# ##############################################################
# Description

# This program installs fsb2.
#
# Edit <CONFIG.sh> first to suit your system.

# ##############################################################
# Usage

#   INSTALL.sh

# ##############################################################
# History

# 2015-10-12: First version.
# 2015-12-29: SuperForth converter.

# ##############################################################

. ./CONFIG.sh

eval ${INSTALLCMD}fsb2.fs $BINDIR/fsb2
eval ${INSTALLCMD}fsb2-abersoft.sh $BINDIR/fsb2-abersoft
eval ${INSTALLCMD}fsb2-abersoft11k.sh $BINDIR/fsb2-abersoft11k
eval ${INSTALLCMD}fsb2-abersoft16k.sh $BINDIR/fsb2-abersoft16k
eval ${INSTALLCMD}fsb2-mgt.sh $BINDIR/fsb2-mgt
eval ${INSTALLCMD}fsb2-superforth.sh $BINDIR/fsb2-superforth
eval ${INSTALLCMD}fsb2-tap.sh $BINDIR/fsb2-tap
