﻿using Courses.Web.Repositories.Interfaces;
using Courses.Web.Utilities.ImageHandler;
using Courses.Web.ViewModels.Courses;
using Microsoft.AspNetCore.Mvc;

namespace Courses.Web.Areas.Admin.Controllers;

public class CourseController : AdminBaseController
{
    #region constructor

    private readonly ICourseRepository _courseRepository;

    public CourseController(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    #endregion

    #region course

    #region courses list

    public async Task<IActionResult> Index(FilterCoursesViewModel filter)
    {
        return View(await _courseRepository.FilterCourses(filter));
    }

    #endregion

    #region create

    [HttpGet]
    public IActionResult CreateCourse()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CreateCourseViewModel course)
    {
        if (ModelState.IsValid)
        {
            string imageName = Guid.NewGuid().ToString("N") + Path.GetExtension(course.Image.FileName);
            course.Image.AddImageToServer(
                imageName,
                PathTools.GetStaticFilePath(StaticFilePath.Course, StaticFileType.Main, StaticFileUseCase.Save),
                400,
                286,
                PathTools.GetStaticFilePath(StaticFilePath.Course, StaticFileType.Thumbnail, StaticFileUseCase.Save),
                null
            );
            await _courseRepository.CreateCourse(course, imageName);
            SetTempDataMessage(TempMessageType.Success, "Course has been created successfully");
            return RedirectToAction("Index", "Course", new { area = "Admin" });
        }

        return View(course);
    }

    #endregion

    #region edit

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await _courseRepository.GetCourseForEdit(id);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, EditCourseViewModel course)
    {
        if (ModelState.IsValid)
        {
            string imageName = course.ImageName;
            if (course.Image != null && course.Image.IsImage())
            {
                imageName = Guid.NewGuid().ToString("N") + Path.GetExtension(course.Image.FileName);
                course.Image.AddImageToServer(
                    imageName,
                    PathTools.GetStaticFilePath(StaticFilePath.Course, StaticFileType.Main, StaticFileUseCase.Save),
                    400,
                    286,
                    PathTools.GetStaticFilePath(StaticFilePath.Course, StaticFileType.Thumbnail, StaticFileUseCase.Save),
                    course.ImageName
                );
            }

            var result = await _courseRepository.EditCourse(course, imageName);
            if (result != null)
            {
                SetTempDataMessage(TempMessageType.Success, "Course has been updated successfully");
                return RedirectToAction("Index", "Course", new { area = "Admin" });
            }
            else
            {
                SetTempDataMessage(TempMessageType.Danger, "An error has been occurred while updating course");
            }
        }

        return View(course);
    }

    #endregion

    #region delete

    public async Task<IActionResult> DeleteCourse(int id)
    {
        var res = await _courseRepository.RemoveCourseById(id);
        if (res) return Json(new { status = "success" });
        return Json(new { status = "not-found" });
    }

    #endregion

    #endregion

    #region sessions

    #region list

    public async Task<IActionResult> CourseSessions(int courseId)
    {
        return PartialView(await _courseRepository.GetAllCourseSessions(courseId));
    }

    #endregion

    #region create

    [HttpGet]
    public async Task<IActionResult> CreateCourseSession(int id)
    {
        var course = await _courseRepository.GetCourseById(id);
        if (course == null) return NotFound();
        ViewBag.Id = id;
        ViewBag.CourseType = course.CourseType;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourseSession(int id, CreateCourseSessionViewModel courseSession)
    {
        var course = await _courseRepository.GetCourseById(id);
        if (course == null) return NotFound();
        ViewBag.Id = id;
        ViewBag.CourseType = course.CourseType;

        if (ModelState.IsValid)
        {
            string videoFileName = "";
            if (courseSession.Video != null)
            {
                videoFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(courseSession.Video.FileName);
                courseSession.Video.AddFileToServer(videoFileName, PathTools.GetStaticFilePath(StaticFilePath.Video, StaticFileType.Main, StaticFileUseCase.Save), null);
            }

            await _courseRepository.CreateCourseSession(id, courseSession, videoFileName);
            TempData["ClosePopup"] = true;
        }

        return View(courseSession);
    }

    #endregion

    #region edit

    [HttpGet]
    public async Task<IActionResult> EditCourseSession(int id)
    {
        var session = await _courseRepository.GetCourseSessionById(id);
        if (session == null) return NotFound();
        ViewBag.Id = id;
        ViewBag.CourseType = session.Course.CourseType;
        return View(new EditCourseSessionViewModel
        {
            Text = session.Text,
            Title = session.Title,
            VideoFileName = session.VideoFileName,
            AdditionalDescription = session.AdditionalDescription,
            DisplayOrder = session.DisplayOrder
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourseSession(int id, EditCourseSessionViewModel session)
    {
        var dbSession = await _courseRepository.GetCourseSessionById(id);
        if (dbSession == null) return NotFound();
        ViewBag.Id = id;
        ViewBag.CourseType = dbSession.Course.CourseType;

        if (ModelState.IsValid)
        {
            string? videoFileName = session.VideoFileName;

            if (session.Video != null)
            {
                videoFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(session.Video.FileName);
                session.Video.AddFileToServer(videoFileName, PathTools.GetStaticFilePath(StaticFilePath.Video, StaticFileType.Main, StaticFileUseCase.Save), dbSession.VideoFileName);
            }

            dbSession.Title = session.Title;
            dbSession.Text = session.Text;
            dbSession.AdditionalDescription = session.AdditionalDescription;
            dbSession.DisplayOrder = session.DisplayOrder;
            dbSession.Text = session.Text;
            dbSession.VideoFileName = videoFileName;
            await _courseRepository.EditCourseSession(dbSession);
            TempData["ClosePopup"] = true;
        }

        return View(session);
    }

    #endregion
    
    #region delete

    public async Task<IActionResult> DeleteCourseSession(int id)
    {
        var res = await _courseRepository.RemoveCourseSessionById(id);
        if (res) return Json(new { status = "success" });
        return Json(new { status = "not-found" });
    }

    #endregion

    #endregion
}