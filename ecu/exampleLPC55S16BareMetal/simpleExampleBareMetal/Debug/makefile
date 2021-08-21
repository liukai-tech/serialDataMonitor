################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.init

RM := rm -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include utilities/subdir.mk
-include startup/subdir.mk
-include src/subdir.mk
-include drivers/subdir.mk
-include device/subdir.mk
-include component/uart/subdir.mk
-include component/lists/subdir.mk
-include board/subdir.mk
-include LPC55S16/drivers/subdir.mk
-include subdir.mk
-include objects.mk

ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
endif

-include ../makefile.defs

# Add inputs and outputs from these tool invocations to the build variables 

# All Target
all: simpleExampleBareMetal.axf

# Tool invocations
simpleExampleBareMetal.axf: $(OBJS) $(USER_OBJS)
	@echo 'Building target: $@'
	@echo 'Invoking: MCU Linker'
	arm-none-eabi-gcc -nostdlib -L"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/LPC55S16/mcuxpresso" -Xlinker -Map="simpleExampleBareMetal.map" -Xlinker --gc-sections -Xlinker -print-memory-usage -Xlinker --sort-section=alignment -Xlinker --cref -mcpu=cortex-m33 -mfpu=fpv5-sp-d16 -mfloat-abi=hard -mthumb -T "simpleExampleBareMetal_Debug.ld" -o "simpleExampleBareMetal.axf" $(OBJS) $(USER_OBJS) $(LIBS)
	@echo 'Finished building target: $@'
	@echo ' '
	$(MAKE) --no-print-directory post-build

# Other Targets
clean:
	-$(RM) $(EXECUTABLES)$(OBJS)$(C_DEPS) simpleExampleBareMetal.axf
	-@echo ' '

post-build:
	-@echo 'Performing post-build steps'
	-arm-none-eabi-size "simpleExampleBareMetal.axf"; # arm-none-eabi-objcopy -v -O binary "simpleExampleBareMetal.axf" "simpleExampleBareMetal.bin" ; # checksum -p LPC55S16 -d "simpleExampleBareMetal.bin";
	-@echo ' '

.PHONY: all clean dependents post-build

-include ../makefile.targets
