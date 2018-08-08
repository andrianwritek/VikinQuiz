﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VikingQuiz.Api.Models;
using VikingQuiz.Api.Repositories;
using VikingQuiz.Api.ViewModels;
using VikingQuiz.Api.Mappers;
using VikingQuiz.Api.Utilities;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VikingQuiz.Api.Controllers
{
    [Route("api/[controller]")]
    public class QuestionsController : Controller
    {
        private readonly QuestionRepo questionRepo;
        private readonly AnswerRepo answerRepo;
        private IEntityMapper<Question, QuestionViewModel> entityToVmMapper;
        private IEntityMapper<QuestionViewModel, Question> vmToEntityMapper;
        private IEntityMapper<Answer, AnswerViewModel> answerToVmMapper;
        private IEntityMapper<AnswerViewModel, Answer> vmToAnswerMapper;

        public QuestionsController(QuestionRepo questionRepo, AnswerRepo answerRepo, IEntityMapper<Question, QuestionViewModel> entityToVmMapper, IEntityMapper<QuestionViewModel, Question> vmToEntityMapper, IEntityMapper<Answer, AnswerViewModel> answerToVmMapper, IEntityMapper<AnswerViewModel, Answer> vmToAnswerMapper)
        {
            this.questionRepo = questionRepo;
            this.answerRepo = answerRepo;
            this.vmToEntityMapper = vmToEntityMapper;
            this.entityToVmMapper = entityToVmMapper;
            this.vmToAnswerMapper = vmToAnswerMapper;
            this.answerToVmMapper = answerToVmMapper;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery(Name = "quiz")] int quizId)
        {
            var questions = questionRepo.GetAllByQuizId(quizId);
            var result = questions
                        .Select(question => {
                                QuestionViewModel questionVM = this.entityToVmMapper.Map(question);
                                questionVM.Answers = this.answerRepo.GetAllAnswers(question.Id)
                                                                    .Select(answer => this.answerToVmMapper.Map(answer))
                                                                    .ToList();
                                return questionVM;
                            })
                        .ToList();
            return Ok(result);
        }

        [HttpPost]
        public IActionResult CreateQuestion([FromBody]QuestionViewModel questionVm, [FromQuery(Name = "quiz")] int quizId)
        {
            Question question = new Question
            {
                Text = questionVm.Text
            };

            Question newQuestion = questionRepo.AddQuestion(quizId, question);
            if (newQuestion == null)
            {
                return BadRequest("The text is already assigned to another question");
            }

            List<AnswerViewModel> realAnswers = new List<AnswerViewModel>();
            
            for(int  i = 0; i < questionVm.Answers.Count; i++)
            {
                AnswerViewModel answerVm = questionVm.Answers[i];
                Answer answer = new Answer
                {
                    Text = answerVm.Text,
                    QuestionId = newQuestion.Id
                };

                int oldId = answerVm.Id;
    
                Answer newAnswer = this.answerRepo.AddAnswer(answer);
                if (newAnswer == null)
                {
                    this.questionRepo.DeleteQuestion(quizId, newQuestion.Id);
                    return BadRequest("You can't have two duplicate answers");
                }

                AnswerViewModel newAnswerVM = this.answerToVmMapper.Map(newAnswer);
                realAnswers.Add(newAnswerVM);

                int newId = newAnswer.Id;

                if (questionVm.CorrectAnswerId == oldId)
                {
                    newQuestion.CorrectAnsId = newId;
                }
            }
            newQuestion = this.questionRepo.UpdateQuestion(newQuestion);
            QuestionViewModel newQuestionVm = entityToVmMapper.Map(newQuestion);
            newQuestionVm.Answers = realAnswers;
            return Created($"/{newQuestionVm.Id}", newQuestionVm);
        }

        [HttpPut]
        public IActionResult UpdateQuestion([FromBody]QuestionViewModel questionVm, [FromQuery(Name = "quiz")] int quizId)
        {
            Question question = vmToEntityMapper.Map(questionVm);
            Question newQuestion = this.questionRepo.UpdateQuestion(question);
            QuestionViewModel newQuestionVm = entityToVmMapper.Map(newQuestion);
            questionVm.Answers.ForEach(answerVm =>
            {
                Answer answer = this.vmToAnswerMapper.Map(answerVm);
                Answer newAnswer = this.answerRepo.UpdateAnswer(answer);
                AnswerViewModel newAnswerViewModel = answerToVmMapper.Map(newAnswer);
                newQuestionVm.Answers.Add(newAnswerViewModel);
            });

            return Accepted($"/{newQuestionVm.Id}", newQuestionVm);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteQuestion(int id, [FromQuery(Name = "quiz")] int quizId)
        {
            questionRepo.DeleteQuestion(quizId, id);
            return Ok();
        }

        [HttpGet("{id}")]
        //[Authorize]
        public IActionResult GetQuestionById(int id)
        {
            Question question = questionRepo.getQuestionById(id);
            if (question == null)
            {
                return NotFound("Question doesn't exist");
            }

            QuestionViewModel questionVm = this.entityToVmMapper.Map(question);
            questionVm.Answers = this.answerRepo.GetAllAnswers(question.Id)
                                                .Select(answer => this.answerToVmMapper.Map(answer))
                                                .ToList();
            return Ok(questionVm);
        }
    }
}