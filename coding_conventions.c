/******************************************
		Coding Conventions: 		
 ******************************************/

/*********************************************************************************************************************************** 
Notes:
	-Don't throw in extra comments that you don't need. Put comments at places that you think that people might not understand.
		too much comments can make code base crowded and unreadable. 

	-Try to avoid heavy functional programming methodology. It looks good and danky but hard to understand when read by others 
		ex: int i = dothis(dothat(1000 * 4)->dosomethingelse(domore()));	// why you do dis?

	-Make code blocks readable, try not to crowd everything in one chunk, leave extra new lines between function calls.	

	-Modularize functions, dont make 1 giant function. Break it up for readers to take a break.
*************************************************************************************************************************************/


/************************************************************************
descriptive / consistent variable names 
	ex: A variable that contains the player's movement speed 
************************************************************************ */
int pms; 			// bad
int p_move_speed;		// good


/********************************************************
private variables declared with a "_" in the front
*********************************************************/
private Transform  _transform;


/********************************************************************** 
descriptive / consistent function names (especially for API calls)
	ex: Function that retrieves data packets from server
***********************************************************************/
public SomeStruct get_p(); 			// needs to make it more explicit
public SomeStruct RetriveServerPackets();	// easier to call from an IDE (Auto complete). 

/****************************************************************
private functions declared with lowercase seperated with "_"
public functions declared Uppcaes for every word
*****************************************************************/
private void _AttemptMove();			// see if the player can move, check for blockables in its path
public void GetPlayerPosition();		// retrieves the 2D vector of where the player is currently at
