using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace SoupToNuts.Phonebook
{
    public class PhonebookUpdateEventArgs : EventArgs
    {
        public ushort Index { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
    }

    public class Phonebook
    {
        public delegate void StatusFeedback (ushort success);

        private List<PhonebookEntry> _entries;
        private string _filename;

        public StatusFeedback OnInitialize { get; set; }
        public StatusFeedback OnSave { get; set; }

        public event EventHandler PhonebookUpdated;

        private int _selection;

        public ushort Selection
        {
            get
            {
                if (_selection < 0)
                    return 0;
                else
                    return (ushort)(_selection + 1);
            }
            set
            {
                if (value < _entries.Count)
                {
                    _selection = value - 1;

                    if (_selection < 0)
                    {
                        SelectedEntryName = "";
                        SelectedEntryNumber = "";
                    }
                    else
                    {
                        SelectedEntryName = _entries[_selection].Name;
                        SelectedEntryNumber = _entries[_selection].Number;
                    }
                }
            }
        }

        public string SelectedEntryName { get; private set; }
        public string SelectedEntryNumber { get; private set; }

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

                if (OnInitialize != null) OnInitialize(1);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Phonebook.Initialize: {0}",
                    e.Message);

                if (OnInitialize != null) OnInitialize(0);
            }
        }

        public void Save()
        {
            try
            {
                using (var stream = File.CreateText(_filename))
                {
                    foreach (var entry in _entries)
                    {
                        stream.WriteLine("{0}|{1}", entry.Name, entry.Number);
                    }
                }

                if (OnSave != null) OnSave(1);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Phonebook.Save: {0}",
                    e.Message);

                if (OnSave != null) OnSave(0);
            }
        }

        public void Add(string name, string number)
        {
            _entries.Add(new PhonebookEntry { Name = name, Number = number });

            if (PhonebookUpdated != null) PhonebookUpdated(this, new PhonebookUpdateEventArgs { Index = (ushort)_entries.Count, Name = name, Number = number });
        }

        public void Remove(ushort index)
        {
            _entries.RemoveAt(index);

            if (PhonebookUpdated != null) PhonebookUpdated(this, new PhonebookUpdateEventArgs { Index = index, Name = "", Number = "" });
        }
    }
}