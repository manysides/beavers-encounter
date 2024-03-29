﻿using System;
using System.Collections.Generic;
using System.Linq;
using Beavers.Encounter.Core;
using SharpArch.Core.PersistenceSupport;

namespace Beavers.Encounter.ApplicationServices
{
    public class RuleTaskDispatcher : ITaskDispatcher
    {
        private const int MaxPoints = Int32.MaxValue;
        private const int MinPoints = Int32.MinValue;

        public Task GetNextTaskForTeam(IRepository<Task> taskRepository, TeamGameState teamGameState, Task oldTask)
        {
            // Получаем все незаблокированные задания для текущей игры
            var gameTasks = taskRepository.GetAll()
                .Where(t => t.Game == teamGameState.Game && !t.Locked);

            // Формируем список номеров групп заданий, с которыми команда уже познакомилась
            List<int> executedGroupTags = new List<int>();
            foreach (TeamTaskState acceptedTask in teamGameState.AcceptedTasks.Where(x => x.Task.GroupTag != 0))
            {
                if (!executedGroupTags.Contains(acceptedTask.Task.GroupTag))
                    executedGroupTags.Add(acceptedTask.Task.GroupTag);
            }

            // Получаем доступные (невыполненные) для команды задания
            List<Task> accessibleTasks = new List<Task>();
            foreach (Task task in gameTasks)
            {
                // Если задание не получено
                // и задание не входит ни в одну "засвеченную" группу,
                // и не запрещена выдача задания текущей команде,
                // то добавляем задание в список
                if (!teamGameState.AcceptedTasks.Any(x => x.Task == task) &&
                    !executedGroupTags.Contains(task.GroupTag) &&
                    !task.NotForTeams.Contains(teamGameState.Team))
                    accessibleTasks.Add(task);
            }

            // Формируем список выполняемых заданий другими командами
            Dictionary<Task, int> executingTasks = new Dictionary<Task, int>();
            foreach (Team team in teamGameState.Game.Teams)
            {
                if (team.TeamGameState != null && team.TeamGameState.ActiveTaskState != null)
                {
                    Task task = team.TeamGameState.ActiveTaskState.Task;
                    if (executingTasks.ContainsKey(task))
                    {
                        executingTasks[task] = executingTasks[task] + 1;
                    }
                    else
                    {
                        executingTasks.Add(task, 1);
                    }
                }
            }

            // Получаем задания выполненные командами, которые помечены опцией "Анти-слив"
            var excludeExecutedTasks = new List<Task>();
            foreach (Team team in teamGameState.Team.PreventTasksAfterTeams)
            {
                if (team.TeamGameState != null)
                {
                    foreach (var task in team.TeamGameState.AcceptedTasks)
                    {
                        if (!excludeExecutedTasks.Contains(task.Task))
                            excludeExecutedTasks.Add(task.Task);
                    }
                }
            }

            List<Task> tasksWithMaxPoints = new List<Task>();
            int maxPoints = 0;

            // Рассчитываем приоритет для каждого задания 
            // и отбираем задания с максимальным приоритетом
            foreach (Task task in accessibleTasks)
            {
                int taskPoints = GetTaskPoints(task, oldTask, executingTasks, excludeExecutedTasks, teamGameState);

                // Не выдавать задание вообще
                if (taskPoints == MinPoints)
                    continue;

                if (taskPoints > maxPoints)
                {
                    maxPoints = taskPoints;
                    tasksWithMaxPoints.Clear();
                    tasksWithMaxPoints.Add(task);
                }
                else if (taskPoints == maxPoints)
                {
                    tasksWithMaxPoints.Add(task);
                }
            }

            // Если заданий с одинаковым приоритетом несколько, 
            // то берем произвольное
            if (tasksWithMaxPoints.Count > 1)
            {
                // Выбираем новое задание из доступных с максимальным приоритетом
                Task newTask = null;
                Random rnd = new Random();
                int indx = rnd.Next(tasksWithMaxPoints.Count);
                int i = 0;
                foreach (Task task in tasksWithMaxPoints)
                {
                    if (i == indx)
                        newTask = task;
                    i++;
                }

                return newTask;
            }
            return tasksWithMaxPoints.Count == 0 ? null : tasksWithMaxPoints.First();
        }

        /// <summary>
        /// Вычисление приоритета для задания.
        /// </summary>
        /// <param name="task">Задание для которого нужно вычислить приоритет.</param>
        /// <param name="oldTask">Предыдущее задание выполненное командой.</param>
        /// <param name="executingTasks">Задания выполняемые в данных момент другими командами.</param>
        /// <param name="excludeExecutedTasks">Задания выполненные командами, которые помечены опцией "Анти-слив".</param>
        /// <returns>Приоритет задания.</returns>
        private static int GetTaskPoints(Task task, Task oldTask, Dictionary<Task, int> executingTasks, List<Task> excludeExecutedTasks, TeamGameState teamGameState)
        {
            int taskPoints = 1000;

            //--------------------------------------------------------------------
            // Если задание связано с предыдущим выданным, то +MaxPoints )))
            if (task.AfterTask != null)
            {
                if (oldTask == null)
                    return MinPoints; // Не выдавать

                if (task.AfterTask == oldTask)
                {
                    var oldTeamTaskState = teamGameState.AcceptedTasks.First(x => x.Task == oldTask);
                    if (task.GiveTaskAfter == GiveTaskAfter.Strictly)
                    {
                        if (oldTeamTaskState.State == (int)TeamTaskStateFlag.Success)
                        {
                            return MaxPoints; // Выдать незамедлительно
                        }
                        return MinPoints; // Не выдавать
                    }
                    if (task.GiveTaskAfter == GiveTaskAfter.StrictlyOrFinaly)
                    {
                        if (oldTeamTaskState.State == (int)TeamTaskStateFlag.Success)
                        {
                            return MaxPoints; // Выдать незамедлительно
                        }
                        return -1000; // Выдать с наименьшим приоритетом

                    }
                    if (task.GiveTaskAfter == GiveTaskAfter.InAnyCase)
                    {
                        return MaxPoints; // Выдать незамедлительно
                    }
                }
                return MinPoints;
            }

            //--------------------------------------------------------------------
            // Если задание типа Челлендж, то +500
            if (task.StreetChallendge)
            {
                taskPoints += 500;
                return taskPoints;
            }

            //--------------------------------------------------------------------
            // Если задание c агентами выполняется другой командой, то -500
            // Задание с агентами одновременно может выполняться только одной командой
            if (task.Agents && executingTasks.ContainsKey(task))
                taskPoints -= 500;

            //--------------------------------------------------------------------
            // Если задание выполнено командами, которые помечены опцией "Анти-слив", то -700
            if (excludeExecutedTasks.Contains(task))
                taskPoints -= 700;

            //--------------------------------------------------------------------
            // Если задание выполняет другая команда, то -50
            if (executingTasks.ContainsKey(task))
                taskPoints -= 50 * executingTasks[task];

            //--------------------------------------------------------------------
            // Если предыдущее задание команды входит в список блокировки по предшествованию, то -400
            if (task.NotAfterTasks.Contains(oldTask))
                taskPoints -= 400;

            //--------------------------------------------------------------------
            // Если хотя бы одно задание из списка блокировки по одновременности выполняется, то -200
            if (task.NotOneTimeTasks.Intersect(executingTasks.Keys).Count() > 0)
                taskPoints -= 200;

            //--------------------------------------------------------------------
            // Если задание содержит коды со сложностью "+500", то +30
            foreach (Code code in task.Codes)
            {
                if (code.Danger == "+500")
                {
                    taskPoints += 30;
                    break;
                }
            }

            //--------------------------------------------------------------------
            // Повышаем приоритет для заданий с бонусами
            // П = П + 10 * (число бонусных кодов в задании)
            int bonusCodes = 0;
            foreach (Code code in task.Codes)
            {
                bonusCodes += code.IsBonus ? 1 : 0;
            }
            taskPoints += bonusCodes * 10;

            //--------------------------------------------------------------------
            // Применяем собственный приоритет задачи
            taskPoints += task.Priority;

            return taskPoints;
        }
    }
}
