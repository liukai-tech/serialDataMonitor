################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../drivers/fsl_clock.c \
../drivers/fsl_common.c \
../drivers/fsl_common_arm.c \
../drivers/fsl_ctimer.c \
../drivers/fsl_flexcomm.c \
../drivers/fsl_gpio.c \
../drivers/fsl_inputmux.c \
../drivers/fsl_reset.c \
../drivers/fsl_usart.c 

OBJS += \
./drivers/fsl_clock.o \
./drivers/fsl_common.o \
./drivers/fsl_common_arm.o \
./drivers/fsl_ctimer.o \
./drivers/fsl_flexcomm.o \
./drivers/fsl_gpio.o \
./drivers/fsl_inputmux.o \
./drivers/fsl_reset.o \
./drivers/fsl_usart.o 

C_DEPS += \
./drivers/fsl_clock.d \
./drivers/fsl_common.d \
./drivers/fsl_common_arm.d \
./drivers/fsl_ctimer.d \
./drivers/fsl_flexcomm.d \
./drivers/fsl_gpio.d \
./drivers/fsl_inputmux.d \
./drivers/fsl_reset.d \
./drivers/fsl_usart.d 


# Each subdirectory must supply rules for building sources it contributes
drivers/%.o: ../drivers/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -std=gnu99 -D__REDLIB__ -DCPU_LPC55S16JBD100 -DCPU_LPC55S16JBD100_cm33 -DMCUXPRESSO_SDK -DSDK_DEBUGCONSOLE=1 -DCR_INTEGER_PRINTF -DPRINTF_FLOAT_ENABLE=0 -D__MCUXPRESSO -D__USE_CMSIS -DDEBUG -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/utilities" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/LPC55S16/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/uart" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/lists" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/device" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/CMSIS" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/board" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/inc" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/utilities" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/LPC55S16/drivers" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/device" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/uart" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/component/lists" -I"/media/olivier/DATADISK/GAW-R/04-Projets/11-serialDataMonitor/ecu/exampleLPC55S16BareMetal/simpleExampleBareMetal/CMSIS" -O0 -fno-common -g3 -mcpu=cortex-m33 -c -ffunction-sections -fdata-sections -ffreestanding -fno-builtin -fmerge-constants -fmacro-prefix-map="../$(@D)/"=. -mcpu=cortex-m33 -mfpu=fpv5-sp-d16 -mfloat-abi=hard -mthumb -D__REDLIB__ -fstack-usage -specs=redlib.specs -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.o)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


