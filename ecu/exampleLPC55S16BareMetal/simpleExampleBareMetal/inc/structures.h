/*
 * structures.h
 *
 *  Created on: 21 août 2021
 *      Author: olivier
 */

#ifndef STRUCTURES_H_
#define STRUCTURES_H_

typedef struct __attribute__ ((packed, aligned(4))) {
	float dt;
	float Kp, Ki, Kd;
	float windup_guard;
	float prop_guard;
} stPIDParams_t;

typedef struct __attribute__ ((packed, aligned(4))) {
	float propError;
	float intError;
	float diffError;
	float lastPropError;
} stPIDVars_t;

typedef struct __attribute__((__packed__)){
	union {
		struct __attribute__((__packed__)){
			bool mode				: 1;
			bool input0				: 1;
			bool input1				: 1;
			bool output0			: 1;
			bool output1			: 1;
			bool error				: 1;
			uint8_t reserved		: 2;
		};
		uint8_t rawByte;
	};
} sMyBools_t;

typedef struct __attribute__ ((packed, aligned(4))) {

	// Boolean values on uint8_t (one char)
	sMyBools_t myBools;

	// Bit banged values for 8 channels (bit=1= channel enabled)
	uint8_t channelsEnabled;

	// One uint8_t
	uint8_t myUInt8;

	// One int8_t
	int8_t myInt8;

	// One uint16_t
	uint16_t myUInt16;

	// One int16_t
	int16_t myInt16;

	// Array of uint16_t
	uint16_t adcRead[8];

	// One uint32_t
	uint32_t myUint32;

	// One int32_t
	int32_t myInt32;

	// One float
	float myFloat;

	// PID variables
	stPIDVars_t PIDVars;

} sVars_t;


typedef struct __attribute__ ((packed, aligned(4))) {

	stPIDParams_t PIDParams;	// PID parameters
	uint16_t PWMFrequency;
	float valvesDitherAmplitude;	// In percentage of the PWM period
									// 0 = 0%, 100 = 25% of the PWM period
	float adcDispersion;

} sPars_t;

typedef struct __attribute__ ((packed, aligned(4))) {
	sPars_t myParameters;
	sVars_t myVariables;
} sGlobVarsPars_t;

#endif /* STRUCTURES_H_ */
