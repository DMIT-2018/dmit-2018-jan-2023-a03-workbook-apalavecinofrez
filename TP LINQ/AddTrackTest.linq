<Query Kind="Program">
  <Connection>
    <ID>d1da1965-ccc1-4057-9529-742ab9ae434f</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>WB320-03\SQLEXPRESS</Server>
    <Database>Chinook2018</Database>
    <DisplayName>ChinookEntity</DisplayName>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <EFVersion>7.0.0-preview.6.22329.4</EFVersion>
      <TrustServerCertificate>True</TrustServerCertificate>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using Chinook;
void Main()
{
	try
	{
		//	this is the DRIVER area.
		
		//  coded and test the AddTrack
		//	The command method will receive no collection bu will receive individual arguments
		//	userName, playlistName, trackID

		//	793 A Castle Full of Rascals
		//	822	A Twist In The Tail
		//	543	Burn
		//	756	Child In Time

		string userName = "HansenB";
		string playlistName = "Jan23A03";
		int trackId = 793;
		//	showing that both the playlist and track does not exist
		Console.WriteLine("Before adding Track");
		PlaylistTrackServices_FetchPlaylist(userName, playlistName).Dump();
		PlaylistTrackServices_AddTrack(userName, playlistName, trackId);
		//	showing that both the playist and tracks now exist
		Console.WriteLine("After adding Track");
		PlaylistTrackServices_FetchPlaylist(userName, playlistName).Dump();
	}

	#region  catch all exceptions
	catch (AggregateException ex)
	{
		foreach (var error in ex.InnerExceptions)
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
	#endregion
}

#region Method
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion


//	check if the incoming data is completed(all parameter exists)
//if a problem exists, throw an ArgumentNullException for missing incoming values
//check that track exists
//does not exist, throws ArgumentException on the trackID
//check to see if playlist exists
//no(new playlist)
//create a new playlist record
//set track number to 1
//yes(appending to existing playlist)
//check if the track already exists on the playlist
//yes
//throw an exception that tracks already exist
//no
//determine the next track number
//add track to playlist tracks
//check for any errors
//yes: throw the list of all collected exceptions
//no: save all work to the database

public void PlaylistTrackServices_AddTrack(string userName, string playlistName, int trackId)
{
	// create local variables
	//  Check to ensure that the track has not been remove from the catelog/library
	bool trackExist = false;
	Playlists playlist = null;
	int trackNumber = 0;
	bool playlistTrackExist = false;
	PlaylistTracks playlistTrack = null;

	#region Business Logic and Parameter Exceptions
	//	create a list<Exception> to contain all discovered errors
	List<Exception> errorList = new List<Exception>();
	
	//	Business Rules
	//	These are processing rules that need to be satisfied for valid data
	//		rule:	a track can only exist once on a playlist
	//		rule:	each track on a playlist is assigned a continous track number
	//		rule:	playlist nane cannot be empty
	//		rule:	track must exist in the tracks table
	//
	//	If the business rules are passed, consider the data valid:
	//		a)	stage your transaction work (Adds, Updates, Deletes)
	//		b)	execute a SINGLE .SaveChanges() - commits to database.
	
	//	We could assume that user name and track ID will always be valid.
	
	// parameter validation
	if(string.IsNullOrWhiteSpace(userName))
	{
		throw new ArgumentNullException("User name is missing");
	}
	if (string.IsNullOrWhiteSpace(playlistName))
	{
		throw new ArgumentNullException("Playlist name is missing");
	}
	#endregion
	
	// check that the incoming data exists
	trackExist = Tracks
		.Where(x => x.TrackId == trackId)
		.Select(x => x.TrackId)
		.Any();
		if(!trackExist)
		{
			throw new ArgumentNullException("Selected track no longer is on the system. Refresh track table");
		}
		
		
	// Business Processes
	// Check if the playlist exists
	playlist = Playlists
				.Where(x => x.Name == playlistName && x.UserName == userName)
				.FirstOrDefault();
	// does not exist
	if (playlist == null)
	{
		playlist = new Playlists()
		{
			Name = playlistName,
			UserName = userName
		};
	// stage (only in memory)
	Playlists.Add(playlist);
	trackNumber = 1;
	}
	else
	{
		// playlist already exists
		// rule:	unique tracks on the playlist
		playlistTrackExist = PlaylistTracks
								.Any(x => x.Playlist.Name == playlistName
										&& x.Playlist.UserName == userName
										&& x.TrackId == trackId);
		if (playlistTrackExist)
		{
			var songName = Tracks
							.Where(x => x.TrackId == trackId)
							.Select(x => x.Name)
							.FirstOrDefault();
			// rule violation
			errorList.Add(new Exception($"Selected track ({songName}) is already on the playlist"));
		}
		else
		{
			trackNumber = PlaylistTracks
							.Where(x => x.Playlist.Name == playlistName
									&& x.Playlist.UserName == userName)
							.Count();
			// increment this by 1
			trackNumber ++;
		}
		
		// add the track to the playlist
		// create an instance for the playlist track
		
		playlistTrack = new PlaylistTracks();
		
		// load values
		playlistTrack.TrackId = trackId;
		playlistTrack.TrackNumber = trackNumber;
		
		// what about the second part of the primary key: PlaylistID?
		// if the playlist exists, then we know the id: playlist.PlaylistId
		// but if the playlist is NEW, we DO NOT KNOW the ID
		
		// In the situation of a NEW playlist, even though we have created the
		// 		playlist instance (see above), it is ONLY staged!!!
		// this means that the actual SQL record has not yet been created
		// this means that the IDENTITY value for the new playlist DOES NOT 
		// 		yet exist.
		// the value on the playlist instance (playlist) is zero (0)
		// 		therefore we havea  serious problem.
		
		// Solution
		// It is built into the Entity Framework software and is based using the
		//		navigational propwerty in the Playlist pointing to its "child"
		
		// Staging a typical Add in the past was to reference the entity and 
		//		use the entity.Add(xxx)
		// If you use this statement, the playlistId would be zero (0)
		// 		causing your transaction to ABORT.
		// why? PKeys cannot be zero (0) (Fkey to PKey problem)
		
		// INSTEAD, do hte staging using the "parent.navChildProperty.Add(xxx)"
		playlist.PlaylistTracks.Add(playlistTrack);
		
		// Staging is complete.
		// Commit the work (Transaction)
		// Commiting the work needs a .SaveChanges()
		// a transaction has ONLY ONE .SaveChanges()
		// BUT, what if you have discovered errors during the business process???
		if (errorList.Count > 0)
		{
			// throw the list of business processing error(s)
			throw new AggregateException("Unable to add new track. Check concerns");
		}
		else
		{
			// consider data valid
			// has passed business processing rules
			SaveChanges();
		}
		
	}
}



public List<PlaylistTrackView> PlaylistTrackServices_FetchPlaylist(string userName, string playlistName)
{
	//	Business Rules
	//	thesee are processing rules that need to be satisfied for valid data.
	//		rule:	search parttern value cannot be empty
	//		rule:	playlist must exist in the database (will be handle on webpage)

	if (string.IsNullOrWhiteSpace(playlistName))
	{
		throw new ArgumentNullException("Playlist name is missing");
	}

	return PlaylistTracks
	.Where(x => x.Playlist.Name == playlistName)
	.Select(x => new PlaylistTrackView
	{
		TrackId = x.TrackId,
		TrackNumber = x.TrackNumber,
		SongName = x.Track.Name,
		Milliseconds = x.Track.Milliseconds
	}).OrderBy(x => x.TrackNumber).ToList();
}