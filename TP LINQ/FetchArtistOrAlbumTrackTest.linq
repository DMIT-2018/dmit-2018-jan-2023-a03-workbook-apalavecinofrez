<Query Kind="Program">
  <Connection>
    <ID>3aef91b8-e674-4713-baf2-236cf5f0145f</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>WC322-07\SQLEXPRESS</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>ChinookSept2018</Database>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using Chinook;
void Main()
{
	try {
		// PlaylistTrackServices_FetchArtistOrAlbumTracks
		// PlaylistTrackServices is the BLL Class
		// FetchArtistOrAlbumTracks is the method name
		
		// create placeholders for our paramaters
		string searchType = "Artist";
		string searchValue = "";
		
		List<TrackSelectionView> tracks = PlaylistTrackServices_FetchArtistOrAlbumTracks (searchType, searchValue);
		tracks.Dump();
	}
	//catch all exceptions
	catch(AggregateException ex) 
	{
		foreach(var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
		
	catch (ArgumentNullException ex) 
	{
		GetInnerException(ex).Message.Dump();
	}
		
	catch (Exception ex) 
	{
		GetInnerException(ex).Message.Dump();
	}
}


#region Method
private Exception GetInnerException(Exception ex) 
{
	while (ex.InnerException != null)
	ex = ex.InnerException;
	return ex;
}

#endregion

public List<TrackSelectionView> PlaylistTrackServices_FetchArtistOrAlbumTracks(string searchType, string searchValue)
{
	// Business Rules
	// These are processing rules that need to be satisfied for valid data
	//		Rule: Search value cannot be empty
	if(string.IsNullOrWhiteSpace(searchValue)) 
	{
		throw new ArgumentNullException("Search pattern name is missing.");
	}
	return Tracks
		.Where(x => searchType == "Artist"? x.Album.Artist.Name.Contains(searchValue): x.Album.Title.Contains(searchValue))
		.Select(x => new TrackSelectionView
		{
			TrackId = x.TrackId,
			SongName = x.Name,
			AlbumTitle = x.Album.Title,
			ArtistName = x.Album.Artist.Name,
			Milliseconds = x.Milliseconds,
			Price = x.UnitPrice
		}).ToList();
}