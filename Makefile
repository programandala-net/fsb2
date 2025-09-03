# Makefile

# Convert <README.adoc> to <README.md> for SourceHut.

# By Marcos Cruz (programandala.net)

# Last modified 20250903T1103+0200.

# Requirements {{{1
# ==============================================================

# Asciidoctor (by Dan Allen, Sarah White et al.)
#   http://asciidoctor.org

# Pandoc (by John MaFarlane)
#   http://pandoc.org

# Config {{{1
# ==============================================================

title := fsb2

# Interface {{{1
# ==============================================================

.PHONY: all
all: readme

.PHONY: readme
readme: README.md

.PHONY: clean
clean:
	rm --force README.md

# AsciiDoc to DocBook {{{1
# ==============================================================

tmp/README.db: README.adoc
	asciidoctor \
		--backend docbook \
		--out-file=$@ $<

# DocBook to CommonMark {{{1
# ==============================================================

# XXX FIXME Somehow `pandoc --from docbook --to commonmark` ignores the main
# title and makes section headings level 1.  This happens still with pandoc
# v3.7.0.2, converting from DocBook to Markdown or CommonMark.  A workaround is
# used with `echo` and `--shift-heading-level-by`:

README.md: tmp/README.db
	echo "# $(title)\n" > $@
	pandoc \
		--from docbook \
		--to commonmark \
		--shift-heading-level-by 1 \
		$< \
		>> $@
