using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaswellSQLConversion.Utils
{
    public class ExceptionHelper
    {
        public static string GetAllFootprints(Exception x)
        {
            var st = new StackTrace(x, true);
            var frames = st.GetFrames();
            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                //traceString.Append("File: " + frame.GetFileName());
                traceString.Append("Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
                traceString.AppendLine();
            }
            traceString.Append("Message: " + x.Message);
            traceString.AppendLine();
            traceString.Append("StackTrace: " + x.StackTrace);
            traceString.AppendLine();
            traceString.Append("InnerException: " + x.InnerException);
            traceString.AppendLine();

            return traceString.ToString();
        }
    }
}
