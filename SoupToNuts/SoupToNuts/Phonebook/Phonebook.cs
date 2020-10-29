using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace SoupToNuts.Phonebook
{
    public class Phonebook
    {
        private List<PhonebookEntry> _entries;
        private string _filename;

        public Phonebook()
        {
            _entries = new List<PhonebookEntry>();
        }

        public void Initialize(string filename)
        {
            _filename = filename;
            
            _entries.Clear();

            try
            {
                using (var stream = File.OpenText(_filename))
                {
                    while (!stream.EndOfStream)
                    {
                        var text = stream.ReadLine();

                        if (text.IndexOf('|') > 0)
                        {
                            var fields = text.Split('|');
                            _entries.Add(new PhonebookEntry {
                                Name = fields[0],
                                Number = fields[1]
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Phonebook.Initialize: {0}",
                    e.Message);
            }
        }
    }
}