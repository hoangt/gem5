CROSS_COMPILER_PATH=/home/hoangt/WORK/TOOLS/Benchmark/cross-compiler

ARM32_HOME=$(CROSS_COMPILER_PATH)/arm32-gcc-linaro
ARM32_BIN_HOME=$(ARM32_HOME)/bin
ARM32_INC_HOME=$(ARM32_HOME)/include
ARM32_LIB_HOME=$(ARM32_HOME)/lib
ARM32_UTL_HOME=$(ARM32_HOME)/arm-linux-gnueabihf

ARM64_HOME=$(CROSS_COMPILER_PATH)/arm64-gcc-linaro
ARM64_BIN_HOME=$(ARM64_HOME)/bin
ARM64_INC_HOME=$(ARM64_HOME)/include
ARM64_LIB_HOME=$(ARM64_HOME)/lib
ARM64_UTL_HOME=$(ARM64_HOME)/aarch64-linux-gnu

RISCV_HOME=/home/hoangt/WORK/TOOLS/risc-v/TOOLCHAIN/rocket-chip/rc-master
RISCV_BIN_HOME=$(RISCV_HOME)/bin
RISCV_INC_HOME=$(RISCV_HOME)/include
RISCV_LIB_HOME=$(RISCV_HOME)/lib

SRCS= $(wildcard *.c)
PROGS= $(patsubst %.c,%,$(SRCS))

CFLAGS = -O2 -static
CINCLS =

ARCH=ARM32
ifeq ($(ARCH), ARM32)
  CC      = $(ARM32_BIN_HOME)/arm-linux-gnueabihf-gcc
	CINCLS += -I$(ARM32_INC_HOME)
  CFLAGS += -L$(ARM32_LIB_HOME)
else
  ifeq ($(ARCH), RISCV)
    CC      = $(RISCV_BIN_HOME)/riscv64-unknown-elf-gcc
    CFLAGS += -L$(RISCV_LIB_HOME)
    CINCLS += -I$(RISCV_INC_HOME)
  else
    ifeq ($(ARCH), AARCH64)
      #CC=$(CC_HOME)/bin/aarch64-linux-gnu-gcc
      CC      = $(ARM64_BIN_HOME)/aarch64-linux-gnu-gcc
		  CFLAGS += -L$(RISCV_LIB_HOME)
		  CINCLS += -I$(RISCV_INC_HOME)
    else
      CC      = gcc
    endif
  endif
endif


build: arm32 x86 aarch64 riscv64

#======
arch:
	@rm -rf $(ARCH) && mkdir -p $(ARCH)
	$(CC) $(CFLAGS) $(CINCLS) stream.c -o $(ARCH)/stream

arm32:
	@rm -rf ARM32
	@make arch ARCH=ARM32

x86:
	@rm -rf X86
	@make arch ARCH=X86

riscv64:
	@rm -rf RISCV
	@make arch ARCH=RISCV

aarch64:
	@rm -rf AARCH64
	@make arch ARCH=AARCH64

#======
clean:
	rm -rf $(ARCH) $(PROGS)

distclean:
	rm -rf x86 X86 arm32 ARM32 aarch64 AARCH64 riscv RISCV

# an example of a more complex build line for the Intel icc compiler
stream.icc: stream.c
	icc -O3 -xCORE-AVX2 -ffreestanding -qopenmp -DSTREAM_ARRAY_SIZE=80000000 -DNTIMES=20 stream.c -o stream.omp.AVX2.80M.20x.icc
