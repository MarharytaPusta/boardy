namespace Boardy.Application
{
    
    public static class ScoreService
    {
        //public static int GetScore(int points)
        //{
        //    return CalculateScore(points, 5);
        //}

        //private static int CalculateScore(int points, int penalty)
        //{
        //    var unused = new List<int>();

        //    return points;
        //}

        public static int GetScore(int points)
        {
            return CalculateScore(points, 5);
        }

        private static int CalculateScore(int points, int penalty)
        {
            int total = points - penalty;

            if (total < 0)
            {
                return 0;
            }

            return total;
        }
    }
}        
