/*------------------------------------------------------------------------------

  SOURCE FILE:              CircularBuffer.cpp

  PROGRAM:                  Defendaman

  FUNCTIONS:                void CBInitialize(CircularBuffer * CBuff, int MaxSize, int ElementSize)
                            void CBFree(CircularBuffer * CBuff)
                            void CBPushBack(CircularBuffer * CBuff, const void *item)
                            void CBPop(CircularBuffer * CBuff, void * item)

  DESIGNER/PROGRAMMER:      Jerry Jia

  NOTES:                    Class to create circular buffers and provide
                            basic functionality for them.

-------------------------------------------------------------------------------*/

#include "CircularBuffer.h"

/*------------------------------------------------------------------------------

  FUNCTION:                   CBInitialize

  DESIGNER/PROGRAMMER:        Jerry Jia

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  void CBInitialize(CircularBuffer * CBuff,
                              int MaxSize, int ElementSize)
                              CBuff - The pointer to the circular buffer structure to intialize.
                              MaxSize - The maximum size of the circular buffer array.
                              ElementSize - The size of each index of the circular buffer array.

  RETURNS:                    void

  NOTES:                      Initialize the circulare buffer given in the
                              parameters based on the buffer size and element
                              size given.

-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   CBFree

  DESIGNER/PROGRAMMER:        Jerry Jia

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  void ServerUDP::Broadcast
                              (const char* message, sockaddr_in * excpt)

  RETURNS:                    void

  NOTES:                      Free all resourses used by the circular buffer.

-------------------------------------------------------------------------------*/
void CBFree(CircularBuffer * CBuff)
{
    free(CBuff->buffer);
}
/*------------------------------------------------------------------------------

  FUNCTION:                   CBPushBack

  DESIGNER/PROGRAMMER:        Jerry Jia

  Revision History:           2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  void CBPushBack(CircularBuffer * CBuff, const void *item)
                                CBuff - The pointer to the circular buffer to store data.
                                item - The pointer to the data that will be added to the circular buffer.

  RETURNS:                     void

  NOTES:                       Add data to the end of the circular buffer.

-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   CBPop

  DESIGNER/PROGRAMMER:        Jerry Jia

  Revision History:           2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  void CBPop(CircularBuffer * CBuff, void * item)
                              (const char* message, sockaddr_in * excpt)

  RETURNS:                    void

  NOTES:                      Remove data from the beginning of the circular buffer.

-------------------------------------------------------------------------------*/
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
