################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../board/clock_config.c \
../board/peripherals.c \
../board/pin_mux.c 

OBJS += \
./board/clock_config.o \
./board/peripherals.o \
./board/pin_mux.o 

C_DEPS += \
./board/clock_config.d \
./board/peripherals.d \
./board/pin_mux.d 


# Each subdirectory must supply rules for building sources it contributes
board/%.o: ../board/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -std=gnu99 -D__REDLIB__ -DCPU_LPC55S16JBD100 -DCPU_LPC55S16JBD100_cm33 -DMCUXPRESSO_SDK -DSDK_DEBUGCONSOLE=1 -DCR_INTEGER_PRINTF -DPRINTF_FLOAT_ENABLE=0 -D__MCUXPRESSO -D__USE_CMSIS -DDEBUG -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/utilities" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/LPC55S16/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/uart" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/lists" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/device" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/CMSIS" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/board" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/inc" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/utilities" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/LPC55S16/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/device" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/uart" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/lists" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/CMSIS" -O0 -fno-common -g3 -mcpu=cortex-m33 -c -ffunction-sections -fdata-sections -ffreestanding -fno-builtin -fmerge-constants -fmacro-prefix-map="../$(@D)/"=. -mcpu=cortex-m33 -mfpu=fpv5-sp-d16 -mfloat-abi=hard -mthumb -D__REDLIB__ -fstack-usage -specs=redlib.specs -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.o)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


