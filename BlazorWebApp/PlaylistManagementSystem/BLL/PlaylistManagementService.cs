using PlaylistManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManagementSystem.BLL
{
    public class PlaylistManagementService
    {
        //  fetch the playlist
        public List<PlaylistTrackView> FetchPlaylist(string userName, string playlistName)
        {
            return null;
        }
        //  fetch artist or album tracks
        public List<TrackSelectionView> FetchArtistOrAlbumTracks(string searchType, string searchValue)
        {
            return null;
        }

        //  add track
        public void AddTracks(string userName, string playlistName, int trackId)
        {
            
        }
        public void RemoveTracks(int playlistID, List<int> trackIds)
        {

        }
        public void MoveTracks(int playlistId, List<MoveTrackView> moveTracks)
        {

        }


    }
}
