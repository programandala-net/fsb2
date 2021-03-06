#!/bin/sh

# INSTALL.sh

# This file is part of fsb2
# http://programandala.net/en.program.fsb2.html

# ===============================================================
# Author and license

# Copyright (C) 2015 Marcos Cruz (programandala.net)

# You may do whatever you want with this work, so long as you
# retain the copyright notice(s) and this license in all
# redistributed copies and derived works. There is no warranty.

# ===============================================================
# Description

# This program installs fsb2.
#
# Edit <CONFIG.sh> first to suit your system.

# ===============================================================
# Usage

# INSTALL.sh

# ===============================================================
# History

# 2015-10-12: First version.
# 2015-12-29: Add SuperForth converter.
# 2016-08-03: Add version file and TRD converter.
# 2016-08-11: Add <fsb2-trd.track_0.fs>.
# 2016-08-14: Add <fsb2-dsk.sh> and <fb2dsk.fs>.

# ===============================================================

. ./CONFIG.sh

eval ${INSTALLCMD}fsb2.fs $BINDIR/fsb2
eval ${INSTALLCMD}fsb2_VERSION.fs $BINDIR
eval ${INSTALLCMD}fsb2-abersoft.sh $BINDIR/fsb2-abersoft
eval ${INSTALLCMD}fsb2-abersoft11k.sh $BINDIR/fsb2-abersoft11k
eval ${INSTALLCMD}fsb2-abersoft16k.sh $BINDIR/fsb2-abersoft16k
eval ${INSTALLCMD}fsb2-mgt.sh $BINDIR/fsb2-mgt
eval ${INSTALLCMD}fsb2-superforth.sh $BINDIR/fsb2-superforth
eval ${INSTALLCMD}fsb2-tap.sh $BINDIR/fsb2-tap
eval ${INSTALLCMD}fsb2-trd.sh $BINDIR/fsb2-trd
eval ${INSTALLCMD}fsb2-trd.track_0.fs $BINDIR
eval ${INSTALLCMD}fsb2-dsk.sh $BINDIR/fsb2-dsk
eval ${INSTALLCMD}fb2dsk.fs $BINDIR/fb2dsk
