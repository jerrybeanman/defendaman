/**********************************************************
Project: Defendaman
Source File: CircularBuffer.cpp
Revision History:
Date        Author      Description
2016-03-09  Gabriel Lee Added function headers and comments. 
Description: Class to create circular buffers and provide
             basic functionality for them.
**********************************************************/
#include "CircularBuffer.h"

/**********************************************************
Description: Initialize the circulare buffer given in the 
             parameters based on the buffer size and element
             size given.
Parameters:
    CBuff - The pointer to the circular buffer structure to intialize.
    MaxSize - The maximum size of the circular buffer array.
    ElementSize - The size of each index of the circular buffer array.
Returns: void
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
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

/**********************************************************
Description: Free all resourses used by the circular buffer.
Parameters:
    CBuff - The pointer to the circular buffer to release resources.
Returns: void
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
void CBFree(CircularBuffer * CBuff)
{
    free(CBuff->buffer);
}

/**********************************************************
Description: Add data to the end of the circular buffer.
Parameters: 
    CBuff - The pointer to the circular buffer to store data.
    item - The pointer to the data that will be added to the circular buffer. 
Returns: void
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
void CBPushBack(CircularBuffer * CBuff, const void *item)
{
    /* Comment this out if we want the head to overwrite the tail */
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

/**********************************************************
Description: Remove data from the beginning of the circular buffer.
Parameters:
    CBuff - The pointer to the circular buffer to remove data.
    itm - The pointer to store the data removed from the circular buffer.
Returns: void
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
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
