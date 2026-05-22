/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2025
 *	
 *	"ITranslatableReferencer.cs"
 * 
 *	An interface used to reference ITranslatable implementations that are not MonoBehaviours within a scene.
 * 
 */


namespace AC
{

	public interface ITranslatableReferencer
	{

		ITranslatable[] GetTranslatables();

	}

}