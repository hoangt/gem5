#====
gem5_home   = /home/hoangt/WORK/TOOLS/Gem5/gem5-base/gem5
gem5_build  = $(gem5_home)/build/ARM
gem5_script = $(gem5_home)/configs/example/se.py

#====
output_path = $(gem5_home)/output/ARM/simple_tests
test_app    = $(gem5_home)/tests/test-progs/hello/bin/arm/linux/hello


#====
num_cpus      = 1
rcs_file      = hack_back_ckpt

linaromin_img = linaro-minimal-aarch64.img
parsec3.0_img = parsec3.0-linaro-minimal-aarch64.img

gem5_cpt_opts = --machine-type=VExpress_GEM5_V1 \
								--dtb-filename=armv8_gem5_v1_$(num_cpus)cpu.dtb \
								--kernel=vmlinux.vexpress_gem5_v1_64 \
								--disk-image=$(linaromin_img)\
								--script=configs/boot/$(rcs_file).rcS

gem5_sim_opts = --machine-type=VExpress_GEM5_V1 \
								--dtb-filename=armv8_gem5_v1_$(num_cpus)cpu.dtb \
								--kernel=vmlinux.vexpress_gem5_v1_64 \
								--disk-image=$(linaromin_img) \
								--checkpoint-dir=checkpoint/armv8_gem5_v1_$(num_cpus)cpu \
								--script=configs/boot/$(rcs_file).rcS

#====
print-debug:
	 ./build/ARM/gem5.debug --debug-help

#====
se-hello:
	echo "Running Hello test in SE mode ..."
	$(gem5_build)/gem5.debug \
  --debug-flags=O3CPUAll \
  --debug-start=50000 \
  --debug-file=$(output_path)/my_trace.out \
  $(gem5_script) \
	--cpu-type=O3_ARM_v7a_3 \
	--caches \
	-c $(test_app)

#====
fs-cpt:
	echo ">>> Creating checkpoing for armv8_gem5_v1_$(num_cpus)cpu ..."
	echo ">>> Disk image path is ${M5_PATH}"
	rm -rf checkpoint/armv8_gem5_v1_$(num_cpus)cpu
	./build/ARM/gem5.opt \
	--outdir=checkpoint/armv8_gem5_v1_$(num_cpus)cpu \
	--redirect-stdout --stdout-file=gem5_out.txt \
	--redirect-stderr --stderr-file=gem5_err.txt \
	configs/example/fs.py $(gem5_cpt_opts)
	@echo ">>> Created checkpoint in checkpoint/armv8_gem5_v1_$(num_cpus)cpu"

fs-sim:
	@echo ">>> Recovering from checkpoint and running simulation for armv8_gem5_v1_$(num_cpus)cpu ..."
	@echo ">>> Disk image path is ${M5_PATH}"
	rm -rf trash/armv8_gem5_v1_$(num_cpus)cpu
	./build/ARM/gem5.opt \
	--outdir=trash/armv8_gem5_v1_$(num_cpus)cpu \
	--redirect-stdout --stdout-file=gem5_out.txt \
	--redirect-stderr --stderr-file=gem5_err.txt \
	configs/example/fs.py $(gem5_sim_opts)
	@echo ">>> Simulation armv8_gem5_v1_$(num_cpus)cpu is DONE !!!"

fs-cpt-1cpu:
	@make fs-cpt num_cpus=1 rcs_file=hack_back_ckpt

fs-sim-1cpu:
	@make fs-sim num_cpus=1 rcs_file=se_benchmarks

fs-cpt-8cpu:
	@make fs-cpt num_cpus=8 rcs_file=hack_back_ckpt

fs-sim-8cpu:
	@make fs-sim num_cpus=8 rcs_file=se_benchmarks

#====
fs-cpt-server:
	rm -rf checkpoint/aarch64_gem5_server
	./build/ARM/gem5.opt \
	--outdir=checkpoint/aarch64_gem5_server \
	--redirect-stdout --stdout-file=gem5_out.txt \
	--redirect-stderr --stderr-file=gem5_err.txt \
	configs/example/fs.py \
	--machine-type=VExpress_EMM64 \
	--dtb=aarch64_gem5_server.dtb \
	--kernel=vmlinux.vexpress_emm64 \
	--disk-image=$(linaromin_img) \
	--script=configs/boot/hack_back_ckpt.rcS

fs-sim-server:
	rm -rf trash/aarch64_gem5_server
	./build/ARM/gem5.opt \
	--outdir=trash/aarch64_gem5_server \
	--redirect-stdout --stdout-file=gem5_out.txt \
	--redirect-stderr --stderr-file=gem5_err.txt \
	configs/example/fs.py \
	--machine-type=VExpress_EMM64 \
	--dtb=aarch64_gem5_server.dtb \
	--kernel=vmlinux.vexpress_emm64 \
	--disk-image=$(linaromin_img) \
	--checkpoint-dir=checkpoint/aarch64_gem5_server \
	--script=configs/boot/se_benchmarks.rcS

fs-sim-server-nocpt:
	rm -rf trash/aarch64_gem5_server
	./build/ARM/gem5.opt \
	--outdir=trash/aarch64_gem5_server_nocpt \
	--redirect-stdout --stdout-file=gem5_out.txt \
	--redirect-stderr --stderr-file=gem5_err.txt \
	configs/example/fs.py \
	--machine-type=VExpress_EMM64 \
	--dtb=aarch64_gem5_server.dtb \
	--kernel=vmlinux.vexpress_emm64 \
	--disk-image=$(linaromin_img) \
	--script=configs/boot/se_benchmarks.rcS

#====
trace-gen:
	echo "Run test ..."
	@rm -rf $(output_path)/*
	$(gem5_build)/gem5.opt \
	--outdir=$(output_path) \
	--debug-flags=MemoryAccess,ExecTicks \
	--debug-file=benchmark.trace \
  $(gem5_script) \
	--cpu-type=DerivO3CPU \
	--caches \
	--l2cache \
	--l3cache \
	--l1trace \
	--l2trace \
	--l3trace \
	--mem-type=SimpleMemory \
	--cmd=tests/test-progs/hello/bin/arm/linux/hello

trace-proc:
	@echo "Analyzing elastic traces ... "
	./util/decode_inst_trace.py      $(output_path)/system.cpu.traceListener.inst_trace.proto.gz $(output_path)/inst_trace.txt
	./util/decode_inst_trace.py      $(output_path)/system.cpu.traceListener.data_trace.proto.gz $(output_path)/data_trace.txt
	./util/decode_inst_dep_trace.py  $(output_path)/system.cpu.traceListener.inst_trace.proto.gz $(output_path)/inst_dep_trace.txt
	./util/decode_inst_dep_trace.py  $(output_path)/system.cpu.traceListener.data_trace.proto.gz $(output_path)/data_dep_trace.txt
	./util/decode_packet_trace.py    $(output_path)/system.cpu.traceListener.inst_trace.proto.gz $(output_path)/inst_packet_trace.txt
	./util/decode_packet_trace.py    $(output_path)/system.cpu.traceListener.data_trace.proto.gz $(output_path)/data_packet_trace.txt
