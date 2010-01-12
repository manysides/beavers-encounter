using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Beavers.Encounter.ApplicationServices;
using Beavers.Encounter.Core;
using Beavers.Encounter.Core.DataInterfaces;
using Beavers.Encounter.Common.MvcContrib;
using Beavers.Encounter.Web.Controllers.Filters;
using SharpArch.Core;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Web.NHibernate;

namespace Beavers.Encounter.Web.Controllers
{
    [TeamGameboard]
    [Authorize]
    [LockIfGameStart]
    [HandleError]
    public class TeamGameboardController : BaseController
    {
        private readonly IGameService gameService;
        private readonly IRepository<Team> teamRepository;

        public TeamGameboardController(IRepository<Team> teamRepository, IUserRepository userRepository, IGameService gameService)
            : base(userRepository)
        {
            Check.Require(teamRepository != null, "teamRepository may not be null");
            Check.Require(gameService != null, "gameService may not be null");

            this.teamRepository = teamRepository;
            this.gameService = gameService;
        }

        //
        // GET: /TeamGameboard/Show
        [Transaction]
        public ActionResult Show()
        {
            //if (((User)User).Team == null && ((User)User).Game != null)
            //    return this.RedirectToAction<GamesController>(c => c.CurrentState(((User) User).Game.Id));

            //try
            //{
            //    gameService.Do((User)User);
            //}
            //catch (Exception e)
            //{

            //}

            Team team = teamRepository.Get(User.Team.Id);

            TeamGameboardViewModel viewModel = TeamGameboardViewModel.CreateTeamGameboardViewModel();

            viewModel.ErrorMessage = Message;

            if (team.Game == null)
            {
                viewModel.Message = "��� ��� ��� �������� ����.";
            }
            else if (team.Game.GameState == (int)GameStates.Finished)
            {
                viewModel.Message = "���� ���������.";
                return RedirectToAction("Results");
            }
            else if (team.Game.GameState == (int)GameStates.Planned)
            {
                viewModel.Message = String.Format("���� ������������� �� {0}",
                                                  team.Game.GameDate.ToShortDateString());
            }
            else if (team.Game.GameState == (int)GameStates.Startup)
            {
                viewModel.Message = String.Format("���� �������� � {0}",
                                                  team.TeamGameState.Game.GameDate.TimeOfDay);
            }
            else if (team.TeamGameState.GameDoneTime != null)
            {
                viewModel.Message =
                    "���� ���������. ��� ��� ���� ������ ��� �������, �������� �� ��������� ��� �������:)";
                return RedirectToAction("Results");
            }
            else
            {
                viewModel.TaskNo = team.TeamGameState.AcceptedTasks.Count;
                viewModel.TeamGameState = team.TeamGameState;
                viewModel.ActiveTaskState = team.TeamGameState.ActiveTaskState;
            }

            viewModel.TeamName = team.Name;

            return View(viewModel);
        }

        //
        // POST: /TeamGameboard/SubmitCodes
        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SubmitCodes(string codes)
        {
            if (User.Team == null && User.Game != null)
                return this.RedirectToAction<GamesController>(c => c.CurrentState(User.Game.Id));

            Team team = teamRepository.Get(User.Team.Id);
            try
            {
                gameService.SubmitCode(codes, team.TeamGameState, User);
            }
            catch (MaxCodesCountException e)
            {
                Message = e.Message;
            }
            return this.RedirectToAction(c => c.Show());
        }

        //
        // POST: /TeamGameboard/NextTask
        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult NextTask(int activeTaskStateId)
        {
            if (User.Team == null && User.Game != null)
                return this.RedirectToAction<GamesController>(c => c.CurrentState(User.Game.Id));

            Team team = teamRepository.Get(User.Team.Id);

            if (team.TeamGameState.ActiveTaskState.Id == activeTaskStateId)
            { // TODO: ��������� � gameService
                Task oldTask = team.TeamGameState.ActiveTaskState.Task;
                if (team.TeamGameState.ActiveTaskState.AcceptedCodes.Count(x => x.Code.IsBonus == 0) == team.TeamGameState.ActiveTaskState.Task.Codes.Count(x => x.IsBonus == 0))
                {
                    gameService.CloseTaskForTeam(team.TeamGameState.ActiveTaskState, TeamTaskStateFlag.Success);
                    gameService.AssignNewTask(team.TeamGameState, oldTask);
                }
            }

            return this.RedirectToAction(c => c.Show());
        }

        //
        // POST: /TeamGameboard/NextTask
        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SkipTask()
        {
            if (User.Team == null && User.Game != null)
                return this.RedirectToAction<GamesController>(c => c.CurrentState(User.Game.Id));

            Team team = teamRepository.Get(User.Team.Id);

            { // TODO: ��������� � gameService
                Task oldTask = team.TeamGameState.ActiveTaskState.Task;
                gameService.CloseTaskForTeam(team.TeamGameState.ActiveTaskState, TeamTaskStateFlag.Canceled);
                gameService.AssignNewTask(team.TeamGameState, oldTask);
            }

            return this.RedirectToAction(c => c.Show());
        }

        //
        // POST: /TeamGameboard/AccelerateTask
        [ValidateAntiForgeryToken]
        [Transaction]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AccelerateTask(int activeTaskStateId)
        {
            Team team = teamRepository.Get(User.Team.Id);
            if (team.TeamGameState != null &&
                team.TeamGameState.ActiveTaskState != null &&
                team.TeamGameState.ActiveTaskState.Id == activeTaskStateId &&
                team.TeamGameState.ActiveTaskState.Task.TaskType == (int)TaskTypes.NeedForSpeed)
            {
                gameService.AccelerateTask(team.TeamGameState.ActiveTaskState);
            }

            return this.RedirectToAction(c => c.Show());
        }

        //
        // GET: /TeamGameboard/Results
        [Transaction]
        public ActionResult Results()
        {
            TeamGameboardViewModel model = TeamGameboardViewModel.CreateTeamGameboardViewModel();
            model.GameResults = gameService.GetGameResults(User.Team.Game.Id).Select("", "tasks desc, bonus desc, time asc");
            
            Team team = teamRepository.Get(User.Team.Id);
            model.TeamGameState = team.TeamGameState;

            return View(model);
        }

        public class TeamGameboardViewModel
        {
            private TeamGameboardViewModel() { }

            public static TeamGameboardViewModel CreateTeamGameboardViewModel()
            {
                return new TeamGameboardViewModel();
            }

            public String Message { get; internal set; }
            public String ErrorMessage { get; internal set; }
            public String TeamName { get; internal set; }
            public int TaskNo { get; internal set; }
            public TeamGameState TeamGameState { get; internal set; }
            public TeamTaskState ActiveTaskState { get; internal set; }
            public DataRow[] GameResults { get; set; }
        }
    }
}