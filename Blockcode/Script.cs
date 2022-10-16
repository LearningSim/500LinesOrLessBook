namespace Blockcode
{
    public class Script
    {
        public bool IsScriptDirty { get; private set; }
        public void ScheduleRun()
        {
            IsScriptDirty = true;
        }

        public void Run()
        {
            if (IsScriptDirty)
            {
                IsScriptDirty = false;
            }
        }
    }
}