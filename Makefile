ARCH_LIST = ALPHA \
					 	ALPHA_MOESI_hammer  MIPS \
					 	ALPHA_MESI_Two_Level \
					 	ALPHA_MOESI_CMP_directory \
					 	ALPHA_MOESI_CMP_token \
					 	ARM \
					 	NULL \
					 	NULL_MOESI_CMP_token  SPARC \
					 	NULL_MOESI_hammer \
					 	NULL_MOESI_CMP_directory \
					 	NULL_MESI_Two_Level \
					 	Garnet_standalone \
					 	POWER \
					 	X86 \
					 	X86_MESI_Two_Level \
					 	X86_MOESI_AMD_Base \
					 	HSAIL_X86 \
					 	RISCV

CC=gcc
CXX=g++

#BUILD_LIST = debug opt prof perf fast
BUILD_LIST = prof

#=================================
ARCH = ARM

#=================================
all:	arm x86 riscv

x86:
	@make install ARCH=X86

arm:
	@make install ARCH=ARM

riscv:
	@make install ARCH=RISCV

#=================================
install:
	@for model in $(BUILD_LIST) ; do \
		echo ">>> Start building GEM5 for <$(ARCH)> architecture in <$$model> mode !!!" ; \
		rm -rf build/$(ARCH)/gem5.$$model
		scons -j8  build/$(ARCH)/gem5.$$model CC=$(CC)  CXX=$(CXX); \
		echo ">>> Finished GEM5 for <$(ARCH)> architecture in mode <$$model> mode !!!" ; \
	done

comp-arm:
	@rm -rf build/ARM/gem5.opt
	@scons -j8  build/ARM/gem5.opt CC=$(CC)  CXX=$(CXX)

comp-risc:
	@rm -rf build/RISCV/gem5.opt
	@scons -j8  build/RISCV/gem5.opt CC=$(CC)  CXX=$(CXX)

comp-x86:
	@rm -rf build/X86/gem5.opt
	@scons -j8  build/X86/gem5.opt CC=$(CC)  CXX=$(CXX)

#=================================
clean:
	@rm -rf build

help:
	@grep '^[^#[:space:]].*:' Makefile

#=================================
#include ./make.fs
include ./make_arm64.fs