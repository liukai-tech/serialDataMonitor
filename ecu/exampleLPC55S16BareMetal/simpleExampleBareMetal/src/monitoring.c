/*
 * monitoring.c
 *
 *  Created on: 30 janv. 2021
 *      Author: olivier
 */

#include "stdio.h"
#include "stdbool.h"
#include "stdint.h"
#include "string.h"
#include "math.h"

#include "structures.h"
#include "monitoring.h"

// Global variables and parameters for all the system
extern sGlobVarsPars_t GVP;

// Simulation of changing variables
extern void setVariables( void ); // defined in main.c

extern char msgFromHost[];		// defined in uart.c
extern bool messagePending;		// defined in uart.c
extern void sendDataHost( char* message ); // defined in uart.c

static float dummy;

typedef struct {
	void* data;
	eDataType_t type;
	bool report;
	void (*cb)(uint8_t); // Callback with index in the table as argument
} sVariables_t;

void onUpdateChannelsEnabled( uint8_t idx ) {

	// Code here
}

void onUpdateMyInt16( uint8_t idx ) {

	// Code here
}

void onUpdateValvesPWMEnable( uint8_t idx ) {

	// Code here
}

// Parameters updated to send to FPGA
void onUpdatePWMFrequency( uint8_t idx ) {

	// Code here
}

void onUpdateDitherFrequency( uint8_t idx ) {

	// Code here
}

void onUpdateDitherAmplitude( uint8_t idx ) {

	// Code here
}

/*
 * This is where to put the parameters/variables to report and edit
 */
sVariables_t variables[] = {
	// Booleans on uint8_t (1 byte variables)
	{ (void *) &GVP.myVariables.myBools,			eComDataType_U8, false, NULL },		// 0
	{ (void *) &GVP.myVariables.channelsEnabled,	eComDataType_U8, false, onUpdateChannelsEnabled },
	// 1 byte variable
	{ (void *) &GVP.myVariables.myUInt8,			eComDataType_U8, false, NULL },
	{ (void *) &GVP.myVariables.myInt8,				eComDataType_I8, false, NULL },
	// 2 bytes variables
	{ (void *) &GVP.myVariables.myUInt16,			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.myInt16,			eComDataType_I16, false, onUpdateMyInt16 },	// 5
	{ (void *) &GVP.myVariables.adcRead[0],			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.adcRead[1],			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.adcRead[2],			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.adcRead[3],			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.adcRead[4],			eComDataType_U16, false, NULL },	// 10
	{ (void *) &GVP.myVariables.adcRead[5],			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.adcRead[6],			eComDataType_U16, false, NULL },
	{ (void *) &GVP.myVariables.adcRead[7],			eComDataType_U16, false, NULL },
	// 4 bytes variables
	{ (void *) &GVP.myVariables.myUint32,			eComDataType_U32, false, NULL },
	{ (void *) &GVP.myVariables.myInt32,			eComDataType_I32, false, NULL },	// 15
	// 4 bytes variables (a float is ALWAYS 4bytes)
	{ (void *) &GVP.myVariables.myFloat,			eComDataType_Float, false, NULL },
	{ (void *) &GVP.myVariables.PIDVars.propError,	eComDataType_Float, false, NULL },
	{ (void *) &GVP.myVariables.PIDVars.intError,	eComDataType_Float, false, NULL },
	// My parameters
	{ (void *) &GVP.myParameters.PIDParams.Kp,		eComDataType_Float, false, NULL },
	{ (void *) &GVP.myParameters.PIDParams.Kd,		eComDataType_Float, false, NULL },	// 20
};

static const uint8_t SIZEvariables = sizeof( variables ) / sizeof( sVariables_t );


/*
 * This is where to put the commands for the system
 */
typedef struct {
	void (*cb)(void*);
	bool hasData;
	eDataType_t typeData;
} sCommands_t;

static void cmdSaveParamToFlash( void* pData )
{
	// Code here
}

static void cmdSetReportingAllOff( void* pData ) {

	for (uint32_t i=0; i<SIZEvariables; i++)
		variables[i].report = false;
}

static void cmdSetMyUInt8Data( void* pData )
{
	uint8_t data = *(uint8_t*)pData;
	// Other code here
}

sCommands_t commands[] = {
	{ cmdSaveParamToFlash, false },		// 0
	{ cmdSetReportingAllOff, false, },
	{ cmdSetMyUInt8Data, true, eComDataType_U8 },
};

static const uint8_t sizeCommands = sizeof( commands ) / sizeof( sCommands_t );

/*****************************************************************************
 * Private functions
 ****************************************************************************/
static void variablesSetReporting( char* dataRx, bool set ) {

	uint8_t varIdx = (uint8_t)(hashmap[(uint8_t)dataRx[1]] << 4) | hashmap[(uint8_t)dataRx[2]];
	uint8_t nbIndexes = (uint8_t)(hashmap[(uint8_t)dataRx[3]] << 4) | hashmap[(uint8_t)dataRx[4]];

	for (uint32_t i=varIdx; i<varIdx+nbIndexes; i++)
	if ( i < SIZEvariables )
		variables[i].report = set;
}

static void getValueFromHex( eDataType_t varType, void* value, char* dataRx ) {

	uint8_t var1;
	uint8_t pVar2[2];
	uint8_t pVar4[4];

	switch ( varType ) {

		// mem: 4 bytes types
		case eComDataType_Float:
		case eComDataType_U32:
		case eComDataType_I32:
			// form of '?XXYWWWWWWWW\0' only
			if ( (dataRx[12] == '\0')) {
				pVar4[0] = (uint8_t)(hashmap[(uint8_t)dataRx[4]] << 4) | hashmap[(uint8_t)dataRx[5]];
				pVar4[1] = (uint8_t)(hashmap[(uint8_t)dataRx[6]] << 4) | hashmap[(uint8_t)dataRx[7]];
				pVar4[2] = (uint8_t)(hashmap[(uint8_t)dataRx[8]] << 4) | hashmap[(uint8_t)dataRx[9]];
				pVar4[3] = (uint8_t)(hashmap[(uint8_t)dataRx[10]] << 4) | hashmap[(uint8_t)dataRx[11]];
				// Store the variable
				memcpy( value, pVar4, 4 );
			}
			break;

		// mem: 2 bytes types
		case eComDataType_U16:
		case eComDataType_I16:
			// form of '?XXYWWWW\0' only
			if ( (dataRx[8] == '\0') ) {
				pVar2[0] = (uint8_t)(hashmap[(uint8_t)dataRx[4]] << 4) | hashmap[(uint8_t)dataRx[5]];
				pVar2[1] = (uint8_t)(hashmap[(uint8_t)dataRx[6]] << 4) | hashmap[(uint8_t)dataRx[7]];
				// Store the variable
				memcpy( value, pVar2, 2 );
			}
			break;

		// mem: 1 byte types A
		case eComDataType_U8:
		case eComDataType_I8:
			// form of '?XXYWW\0' only
			if ( (dataRx[6] == '\0') ) {
				var1 = (uint8_t)(hashmap[(uint8_t)dataRx[4]] << 4) | hashmap[(uint8_t)dataRx[5]];
				*((uint8_t *)value) = var1;
			}
			break;
	}
}

static void variablesSetVariable( char* dataRx ) {

	const uint8_t sizevariables = sizeof( variables ) / sizeof( sVariables_t );

	uint8_t varIdx;
	eDataType_t varType;

	varIdx = (uint8_t)(hashmap[(uint8_t)dataRx[1]] << 4) | hashmap[(uint8_t)dataRx[2]];

	if ( varIdx < sizevariables ) {

		varType = (eDataType_t)dataRx[3];

		// Value type to store must be the same as the data type
		if ( varType == variables[varIdx].type ) {
			// GetnStore
			getValueFromHex(varType, variables[varIdx].data, dataRx);
			// Call the callback if any
			if ( variables[varIdx].cb != NULL )
				variables[varIdx].cb(varIdx);
		}
	}
}

static void variablesSend( uint32_t index ) {

	char msgString[15];

	uint8_t var1;
	uint8_t pVar2[2];
	uint8_t pVar4[4];

	msgString[0] = reportValue;
	sprintf( &msgString[1], "%02X", index );
	msgString[3] = variables[index].type;

	switch ( variables[index].type ) {

		// mem: 4 bytes types
		case eComDataType_Float:
		case eComDataType_U32:
		case eComDataType_I32:
			memcpy( pVar4, variables[index].data, 4 );
			sprintf( &msgString[4], "%02X", pVar4[0] );
			sprintf( &msgString[6], "%02X", pVar4[1] );
			sprintf( &msgString[8], "%02X", pVar4[2] );
			sprintf( &msgString[10], "%02X", pVar4[3] );
			msgString[12] = '\n';
			msgString[13] = '\0';
			break;

		// mem: 2 bytes types
		case eComDataType_U16:
		case eComDataType_I16:
			memcpy( pVar2, variables[index].data, 2 );
			sprintf( &msgString[4], "%02X", pVar2[0] );
			sprintf( &msgString[6], "%02X", pVar2[1] );
			msgString[8] = '\n';
			msgString[9] = '\0';
			break;

		// mem: 1 byte types A
		case eComDataType_U8:
		case eComDataType_I8:
			var1 = *((uint8_t *)variables[index].data);
			sprintf( &msgString[4], "%02X", var1 );
			msgString[6] = '\n';
			msgString[7] = '\0';
			break;
	}

	// Send result
	sendDataHost( msgString );
}

/*
 * Seek and send from the sVariables_t struct for the next variable to report.
 * Each time this function is called, the index is set to the next variable
 * whose report bit is set to true and send it. When arrived at the end of struct, go
 * to the beginning as a loop.
 * Detect is no variables have their report bit to true (no endless loop)
 */
static void variablesSendNext( void ) {

	static uint32_t index = 0;
	static uint32_t lastIndex = 0;
	const uint8_t SIZEvariables = sizeof( variables ) / sizeof( sVariables_t );
	bool indexFound = false;

	// Find next index
	while ( true ) {

		if ( ++index == SIZEvariables )
			index = 0;

		if ( variables[index].report ) {
			indexFound = true;
			break;
		}

		if ( index == lastIndex )
			break;
	}

	if ( indexFound ) {
		lastIndex = index;
		variablesSend( index );
	}
}

/*****************************************************************************
 * Public function, called every tick timer
 ****************************************************************************/
/*
 * 1. Report variables every report period (if requested)
 * 2. Interpret and execute command from host
 */
void timer_CB( uint32_t flags ) {

	uint8_t mem[4];
	uint32_t varIdx;
	uint8_t cmdIdx;

	bool skipReport = false;

	// If a msg received, execute it
	if ( messagePending ) {

		messagePending = false;

		switch ( (eHostMonitoringCmd_t)msgFromHost[0] ) {

			case reportValueOn:
				variablesSetReporting( msgFromHost, true );
				break;

			case reportValueOff:
				variablesSetReporting( msgFromHost, false );
				break;

			case reportValueOnce:
				skipReport = true;
				varIdx = (uint8_t)(hashmap[(uint8_t)msgFromHost[1]] << 4) | hashmap[(uint8_t)msgFromHost[2]] ;
				// Send directly the variable
				if ( varIdx < SIZEvariables )
					variablesSend( varIdx );
				break;

			case setValue:
				variablesSetVariable( msgFromHost );
				break;

			case executeCMD:
				cmdIdx = (uint8_t)(hashmap[(uint8_t)msgFromHost[1]] << 4) | hashmap[(uint8_t)msgFromHost[2]];
				if (cmdIdx < sizeCommands) {
					if (!commands[cmdIdx].hasData)
						//Call the function
						commands[cmdIdx].cb(NULL);
					else {
						// Value parameter must be the same as the data type
						if ( (eDataType_t)msgFromHost[3] == commands[cmdIdx].typeData ) {
							// GetnStore the value
							getValueFromHex((eDataType_t)msgFromHost[3], mem, msgFromHost);
							// Call the function
							commands[cmdIdx].cb(mem);
						}
					}
				}
				break;

			default:
				break;
		}
	}

	// Send next monitoring variable
	if ( !skipReport )
		variablesSendNext();

	// DEBUG set values on variables to see
	// something on the monitor :)
	setVariables();
}

