#pragma once
#include <stdlib.h>
#include <string.h>
/* Can be any type */
struct CircularBuffer
{
	void	*	buffer;		   	/* data buffer								*/
	void	*	buffer_end; 	/* end of data buffer						*/
	int		MaxSize;	   	/* maximum number of items in the buffer	*/
	int		Count;		  	/* number of items in the buffer			*/
	int		ElementSize;	/* size of each item in the buffer			*/
	void	*	Front;		  	/* pointer to Front							*/
	void	*	Rear;		     	/* pointer to Rear							*/
};

void CBInitialize(CircularBuffer * CBuff, int MaxSize, int ElementSize);

void CBFree(CircularBuffer * CBuff);

void CBPushBack(CircularBuffer * CBuff, const void * item);

void CBPop(CircularBuffer * CBuff, void * item);
