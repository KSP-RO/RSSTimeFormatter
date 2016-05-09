SHELL=/bin/bash
PROJ_NAME = $(shell basename `pwd`)
CONFIG_DIR = configs
VERSION = $(shell git describe --tags)
BRANCH := $(shell git rev-parse --abbrev-ref HEAD 2>&1)

ifdef TRAVIS_TAG
ZIP_CORE := RSSDateTime-$(TRAVIS_TAG).zip
else
ZIP_CORE := RSSDateTime-$(TRAVIS_BRANCH)_$(TRAVIS_BUILD_NUMBER).zip
endif

release: zip

ifdef TRAVIS_TAG
meta:
	python makeMeta.py $(TRAVIS_TAG)
	cp RSSDateTime.version GameData/RSSDateTime/RSSDateTime.version
else
meta:
endif

zip: meta
	zip $(ZIP_CORE) GameData GameData/RSSDateTime/* GameData/RSSDateTime/Plugins/* 

clean:
	-rm *.zip
	-rm GameData/RSSDateTime/*.version
	-rm GameData/RSSDateTime/*.ckan
	-rm *.version
	-rm *.ckan

