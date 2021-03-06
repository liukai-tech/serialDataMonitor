/*
 * main.c
 *
 *  Created on: 20 août 2021
 *      Author: olivier
 */

#include <stdbool.h>
#include <math.h>

#include "peripherals.h"
#include "pin_mux.h"
#include "clock_config.h"

#include "fsl_usart.h"
#include "fsl_power.h"

#include "structures.h"

//#include "monitoring.h"

// Global variables and parameters for all the system
sGlobVarsPars_t GVP;


// Simulation of changing variables
void setVariables( void ) {

	static uint8_t i = 0;
	static uint32_t counter = 0;
	static uint32_t sp = 0;
	static uint8_t sp2 = 0;
	float x;

	if ( (counter % 100) == 0 )
		GVP.myVariables.myBools.rawByte = i++;

	if ( (counter % 4) == 0 ) {

		GVP.myVariables.adcRead[0] = 30 + (int16_t)(30.0f * sinf( 2*3.14159 * (float)sp / 1000.0f ));
		GVP.myVariables.adcRead[1] = 10 + (int16_t)(10.0f * sinf( 2*3.14159 * sp / 25000.0f + 0.5f));
	}
	if ( (counter % 4) == 1 ) {
		GVP.myVariables.adcRead[2] = 70 + (int16_t)(70.0f * sinf( 2*3.14159 * (float)sp / 560.0f + 1.2f ));
		GVP.myVariables.adcRead[3] = 50 + (int16_t)(50.0f * sinf( 2*3.14159 * (float)sp / 25.0f - 0.5f ));
	}
	if ( (counter % 4) == 2 ) {
		GVP.myVariables.adcRead[4] = 15 + (int16_t)(15.0f * sinf( 2*3.14159 * (float)sp / 560.0f + 1.2f ));
		GVP.myVariables.adcRead[5] = 45 + (int16_t)(45.0f * sinf( 2*3.14159 * (float)sp / 2500.0f - 0.5f ));
	}
	if ( (counter % 4) == 3 ) {
		GVP.myVariables.adcRead[6] = 80 + (int16_t)(80.0f * sinf( 2*3.14159 * (float)sp / 500.0f + 2.5f ));
		GVP.myVariables.adcRead[7] = 22 + (int16_t)(22.0f * sinf( 2*3.14159 * (float)sp++ / 700.0f + 1.8f ));
		GVP.myVariables.adcRead[6] += ((float)rand() / 100000000.0f);
	}

	if ( (counter % 10) == 0 ) {
		x = 2*3.14159 * (sp2++-127) / 10.0f;
		if (x!= 0.0f)
			GVP.myVariables.myFloat = 100 * sinf( x ) / x;
		else
			GVP.myVariables.myFloat = 100.0f;
	}

	counter++;
}

/*!
 * @brief Main function
 */
int main(void)
{
    BOARD_InitBootPins();
    BOARD_InitBootClocks();
    BOARD_InitBootPeripherals();

	// Enable HW timer
	CTIMER_StartTimer(CTIMER0_PERIPHERAL);

    while (1) {

    }
}
