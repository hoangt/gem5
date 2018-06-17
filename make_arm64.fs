#====
gem5_home   = /home/hoangt/WORK/TOOLS/Gem5/arm-gem5-rsk/gem5
gem5_build  = $(gem5_home)/build/ARM
gem5_cmd    = $(gem5_build)/arm.opt
gem5_scr    = $(gem5_home)/configs/arm/starter_fs.py

#====
cpu_type    = "hpi"
num_cores   = 2
output_dir  = $(gem5_home)/output/ARM/$(cpu_type)_$(num_cores)cores_arm64_parsec3.0_starter_fs
cpt_dir     = $(gem5_home)/checkpoint/cpt_$(cpu_type)_$(num_cores)cores_arm64_starter_fs
rcs_dir     = $(gem5_home)/configs/boot
image_dir   = $(gem5_home)/full-system/aarch-system-20170616
image_file  = extended-linaro-minimal-aarch64
cmd_opts	  = --redirect-stdout --stdout-file=gem5_out.txt \
						  --redirect-stderr --stderr-file=gem5_err.txt \

#====
benchmark   = blackscholes
scr_opts    = --num-cores=$(num_cores) \
							--cpu=$(cpu_type) \
							--machine-type=VExpress_GEM5_V1 \
							--dtb-filename=armv8_gem5_v1_$(num_cores)cpu.20170616.dtb \
							--kernel=vmlinux.vexpress_gem5_v1_64 \
							--disk-image=$(image_dir)/disks/${image_file).img

#==== Create Checkpoint for multicores system ====
gen-cpt:
	@gem5_cmd $(cmd_opts) \
						--output=$(cpt_dir) \
						$(gem5_scr) \
						$(scr_opts) \
						--script=$(rcs_dir)/hack_back_ckpt.rcS

gen-cpt-mc:
	@for nc in 1 2 4 8; do \
		make gen-cpt num_cores=$$nc ; \
	done

#==== Simulation using Checkpoint recovery ====
