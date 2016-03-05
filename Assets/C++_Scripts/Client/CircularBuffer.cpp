#include "CircularBuffer.h"

void CBInitialize(CircularBuffer * CBuff, int MaxSize, int ElementSize)
{
	CBuff->buffer = malloc(MaxSize * ElementSize);
	if (CBuff->buffer == NULL)
	{
		return;
	}
	CBuff->buffer_end = (char *)CBuff->buffer + MaxSize * ElementSize;
	CBuff->MaxSize = MaxSize;
	CBuff->Count = 0;
	CBuff->ElementSize = ElementSize;
	CBuff->Front = CBuff->buffer;
	CBuff->Rear = CBuff->buffer;
}

void CBFree(CircularBuffer * CBuff)
{
	free(CBuff->buffer);
}

void CBPushBack(CircularBuffer * CBuff, const void *item)
{
	/* Comment this out if we want the head to overwirte the tail */
	if (CBuff->Count == CBuff->MaxSize)
	{
		return;
	}

	memcpy(CBuff->Front, item, CBuff->ElementSize);
	CBuff->Front = (char *)CBuff->Front +  CBuff->ElementSize;
	if (CBuff->Front == CBuff->buffer_end)
		CBuff->Front = CBuff->buffer;
	CBuff->Count++;
}

void CBPop(CircularBuffer * CBuff, void * item)
{
	if (CBuff->Count == 0)
	{
		return;
	}
	memcpy(item, CBuff->Rear, CBuff->ElementSize);
	CBuff->Rear = (char *)CBuff->Rear + CBuff->ElementSize;
	if (CBuff->Rear == CBuff->buffer_end)
		CBuff->Rear = CBuff->buffer;
	CBuff->Count--;
}
