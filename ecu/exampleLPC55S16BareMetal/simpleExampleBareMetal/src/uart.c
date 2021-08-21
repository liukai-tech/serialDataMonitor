/*
 * uart.c
 *
 *  Created on: 20 août 2021
 *      Author: olivier
 */
#include "fsl_usart.h"
#include "peripherals.h"

#define RX_BUFFER_SIZE 	100

bool messagePending;				// used in monitoring.c, flag for message received from host
char msgFromHost[RX_BUFFER_SIZE];	// used in monitoring.c, message received from host

static char rxCMDBuffer[RX_BUFFER_SIZE]; // Used only here

void FLEXCOMM0_FLEXCOMM_IRQHANDLER(void) {

    static uint32_t rxCMDIndex = 0;

    // Is char received ?
    if ((kUSART_RxFifoNotEmptyFlag | kUSART_RxError) & USART_GetStatusFlags(USART0)) {

    	rxCMDBuffer[rxCMDIndex++] = USART_ReadByte(USART0);

		if ( rxCMDBuffer[rxCMDIndex-1] == '\n') {
			// EOL received, replace it with 0
			rxCMDBuffer[rxCMDIndex-1] = 0;
			// and copy to msgFromHost
			strcpy( msgFromHost, rxCMDBuffer);
			// then reset buffer index
			rxCMDIndex = 0;
			// tell timer_CB in monitor.c that message arrived
			messagePending = true;
		}
    }
    SDK_ISR_EXIT_BARRIER;
}

/*************************************
 * *************************************
 * Send the msg at once since size <= 15 bytes = the TX FIFO size :)
 * No need to test if already transmitting
 * if report period well chosen regarding the baudrate
 * *************************************
 **************************************/
void sendDataHost( char* message ) {

	for ( uint8_t i=0; i< strlen(message); i++)
	    USART_WriteByte(USART0, message[i]);
}
