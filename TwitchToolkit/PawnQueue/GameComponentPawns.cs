using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TwitchToolkit.PawnQueue
{
    public class GameComponentPawns : GameComponent
    {
        private const int DespawnTimeoutTicks = GenDate.TicksPerHour * 2; // 2 in-game hours

        public override void GameComponentTick()
        {
            int ticks = Find.TickManager.TicksGame;
            if (ticks % 1000 != 0)
                return;

            List<Pawn> currentColonists = Find.ColonistBar.GetColonistsInOrder();
            List<string> usernamesToRemove = new List<string>();
            foreach (KeyValuePair<string, Pawn> pair in pawnHistory)
            {
                string username = pair.Key;
                Pawn pawn = pair.Value;
                if (pawn == null)
                {
                    usernamesToRemove.Add(username);
                    continue;
                }
                bool despawned = !pawn.Spawned && !pawn.Destroyed;
                if (!despawned)
                {
                    if (pawnDespawnTicks.Remove(pawn))
                    {
                        Helper.Log($"Despawned colonist {pawn}: unassign timeout started at {ticks} ticks, NO LONGER DESPAWNED");
                    }
                }
                if (!currentColonists.Contains(pawn))
                {
                    if (despawned)
                    {
                        // If despawned pawn is neither discarded nor destroyed,
                        // only remove if the pawn is still despawned after a timeout period.
                        // Workaround for RimWorld of Magic temporarily despawning pawns during certain spells.
                        if (pawnDespawnTicks.TryGetValue(pawn, out int despawnTicks))
                        {
                            Helper.Log($"Despawned colonist {pawn}: unassign timeout started at {ticks} ticks, " +
                                $"{DespawnTimeoutTicks - (ticks - despawnTicks)} ticks remaining");
                            if (ticks - despawnTicks > DespawnTimeoutTicks)
                            {
                                usernamesToRemove.Add(username);
                            }
                        }
                        else
                        {
                            Helper.Log($"Despawned colonist {pawn}: unassign timeout started at {ticks} ticks, " +
                                $"{DespawnTimeoutTicks} ticks remaining");
                            pawnDespawnTicks[pawn] = ticks;
                        }
                    }
                    else // any other reason colonist is no longer in colonist bar (imprisoned, wildman, kidnapped, destroyed, desiccated)
                    {
                        usernamesToRemove.Add(username);
                    }
                }
            }

            foreach (string username in usernamesToRemove)
            {
                UnassignPawn(username);
            }
        }

        public void AssignUserToPawn(string username, Pawn pawn)
        {
            UnassignPawn(pawn);
            pawnHistory.Add(username, pawn);
            if (ViewerNameQueue.Contains(username))
            {
                ViewerNameQueue.Remove(username);
            }
        }

        public bool HasUserBeenNamed(string username)
        {
            return pawnHistory.ContainsKey(username);
        }

        public bool HasPawnBeenNamed(Pawn pawn)
        {
            return pawnHistory.ContainsValue(pawn);
        }

        public string UserAssignedToPawn(Pawn pawn)
        {
            if (!HasPawnBeenNamed(pawn)) return null;
            return pawnHistory.FirstOrDefault(s => s.Value == pawn).Key;
        }

        public Pawn PawnAssignedToUser(string username)
        {
            if (pawnHistory.ContainsKey(username))
            {
                return pawnHistory[username];
            }
            return null;
        }

        public void UnassignPawn(Pawn pawn)
        {
            foreach (KeyValuePair<string, Pawn> pair in pawnHistory)
            {
                if (pair.Value == pawn)
                {
                    string username = pair.Key;
                    Helper.Log($"Unassigning colonist {pawn} from user {username}");
                    pawnHistory.Remove(username);
                    return;
                }
            }
        }

        public void UnassignPawn(string username)
        {
            if (pawnHistory.TryGetValue(username, out Pawn pawn))
            {
                Helper.Log($"Unassigning colonist {pawn} from user {username}");
                pawnHistory.Remove(username);
            }
        }

        public bool UserInViewerQueue(string username)
        {
            return ViewerNameQueue.Contains(username);
        }

        public void AddViewerToViewerQueue(string username)
        {
            if (!UserInViewerQueue(username))
            {
                ViewerNameQueue.Add(username);
            }
        }

        public string GetNextViewerFromQueue()
        {
            if (ViewerNameQueue.Count < 1)
            {
                return null;
            }
            return ViewerNameQueue[0];
        }

        public string GetRandomViewerFromQueue()
        {
            if (ViewerNameQueue.Count < 1)
            {
                return null;
            }
            return ViewerNameQueue[Verse.Rand.Range(0, ViewerNameQueue.Count - 1)];
        }

        public int ViewersInQueue()
        {
            return ViewerNameQueue.Count;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pawnHistory, "pawnHistory", LookMode.Value, LookMode.Reference, ref pawnNames, ref listPawns);
            Scribe_Collections.Look(ref viewerNameQueue, "viewerNameQueue", LookMode.Value);
        }

        public List<string> ViewerNameQueue
        {
            get
            {
                if (viewerNameQueue == null)
                {
                    viewerNameQueue = new List<string>();
                }

                return viewerNameQueue;
            }
        }

        public Dictionary<string, Pawn> pawnHistory = new Dictionary<string, Pawn>();
        public List<string> viewerNameQueue = new List<string>();

        public List<Pawn> listPawns = new List<Pawn>();
        public List<string> pawnNames = new List<string>();

        private Dictionary<Pawn, int> pawnDespawnTicks = new Dictionary<Pawn, int>();
    }
}
