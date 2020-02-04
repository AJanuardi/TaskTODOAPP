  
using System.IO;

namespace TODO
{
    public class Token
    {
        public string token{get; set;}

        public string GetToken()
        {
            return System.IO.File.ReadAllText("token.txt");
        }

        public bool SaveToken()
        {
            if(token != null)
            {
                var File = new StreamWriter("token.txt");
                File.Write(token);
                File.Close();
                return true;
            }

            else
                return false;
            
        }
    }
}