using System;
using System.IO;
using NLog.Web;
using System.Linq;
using System.Collections.Generic;



namespace MediaLibrary
{
    public class MovieFile
    {
        public string filePath { get; set; }
        public List<Movie> Movies { get; set; }
    
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        List<Movie> movies = new List<Movie>();
        string csvHeader = "";
        string movieFile;
        public MovieFile(string movieFile)
        {
            this.movieFile = movieFile;
            this.loadMovies();
        }

        public void addMovieToList(Movie movie) {
            this.movies.Add(movie);
        }

         public void loadMovies() {
            try
            {
                StreamReader sr1 = new StreamReader(this.movieFile);
                if (File.Exists(this.movieFile)) {
                    while (!sr1.EndOfStream)
                    {
                        Movie movie = new Movie();
                        string line = sr1.ReadLine();
                        int idx = line.IndexOf('"');
                        
                        if (idx == -1)
                        {
                            // no quote = no comma or quote in movie title
                            // movie details are separated with comma(,)
                            string[] movieDetails = line.Split(',');
                            movie.mediaId = UInt64.Parse(movieDetails[0]);
                            movie.title = movieDetails[1];

                            String[] genreData = movieDetails[2].Split("|");
                            movie.genres = genreData.ToList<string>();
                            movie.director = movieDetails.Length > 3 ? movieDetails[3] : "unassigned";
                            movie.runningTime = movieDetails.Length > 4 ? TimeSpan.Parse(movieDetails[4]) : new TimeSpan(0);
                        }
                        else
                        {
                            // quote = comma or quotes in movie title
                            // extract the movieId
                            movie.mediaId = UInt64.Parse(line.Substring(0, idx - 1));
                            // remove movieId and first comma from string
                            line = line.Substring(idx);
                            // find the last quote
                            idx = line.LastIndexOf('"');
                            // extract title
                            movie.title = line.Substring(0, idx + 1);
                            // remove title and next comma from the string
                            line = line.Substring(idx + 2);
                            // split the remaining string based on commas
                            string[] details = line.Split(',');
                            // the first item in the array should be genres 
                            String[] genreData = details[0].Split("|");
                            movie.genres = genreData.ToList<string>();
                            // if there is another item in the array it should be director
                            movie.director = details.Length > 1 ? details[1] : "unassigned";
                            // if there is another item in the array it should be run time
                            movie.runningTime = details.Length > 2 ? TimeSpan.Parse(details[2]) : new TimeSpan(0);
                        }

                        this.addMovieToList(movie);
                        
                    }
                } else {
                    logger.Error("File does not exist: " + this.movieFile);
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(ex.Message);
                logger.Error("There has been an Exception " + ex.Message);
            }
        }

        public void printMovies() {
            foreach (Movie movie in this.movies) {
                string genres = string.Join(", ", movie.genres);
                Console.WriteLine($"Id: {movie.mediaId}\nTitle: {movie.title}\nDirector: {movie.director}\nRunning Time: {movie.runningTime}\nGenres: {genres}\n\n");
            }
        }

        public void userAddMovie() {
            //ask user for certain things
            Console.WriteLine("Enter movie title.");
            string title = Console.ReadLine();

            Console.WriteLine("Enter movie director.");
            string director = Console.ReadLine();

            Console.WriteLine("Enter running time. (hh:mm:ss)");
            TimeSpan timespan = TimeSpan.Parse(Console.ReadLine());
            // Ask for genre ( can be multiple so ask if they want to add more)
            Boolean done = false;
            List<string> genre = new List<string>();
            while(!done) {
                Console.WriteLine("Please enter a genre");
                genre.Add(Console.ReadLine());
                Console.WriteLine("would you like to add more? yes/no");
                string userResponse = Console.ReadLine();
                // ask user if they want to add more
                if (userResponse != "yes") {
                    done = true;
                }

            }

            Movie movie = new Movie();
            movie.mediaId = getNextId();
            movie.title = title;
            movie.director = director;
            movie.genres = genre;
            movie.runningTime = timespan;

            this.addMovieToList(movie);
        }

        public UInt64 getNextId() {
            return this.movies[this.movies.Count-1].mediaId + 1;
        }

        public void findMovies() {
            var moviesFound = this.Movies.Where(m => m.title.Contains("(1921)"));
            Console.WriteLine(moviesFound.Count() + " Matches Found");
            foreach(Movie m in moviesFound)
            {
                Console.WriteLine($" - {m.title}");
            }
        }
    }
}