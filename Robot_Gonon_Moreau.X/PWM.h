/* 
 * File:   PWM.h
 * Author: TABLE 6
 *
 * Created on 3 octobre 2023, 08:53
 */

#ifndef PWM_H
#define	PWM_H

#define MOTEUR_DROIT 0
#define MOTEUR_GAUCHE 1

void InitPWM(void);
void PWMSetSpeed(float vitesseEnPourcents, int Moteur);

#ifdef	__cplusplus
extern "C" {
#endif




#ifdef	__cplusplus
}
#endif

#endif	/* PWM_H */

