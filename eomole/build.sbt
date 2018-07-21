lazy val root = project.settings(
  name := "icfpc2018",
  version := "0.1",
  scalaVersion := "2.12.6"
).aggregate(ai, js)

lazy val ai = project.in(file("modules/ai")).settings(
  scalaVersion := "2.12.6"
).dependsOn(interpreter)

lazy val js = project.in(file("modules/js")).enablePlugins(ScalaJSPlugin).settings(
  scalaVersion := "2.12.6"
).dependsOn(interpreter)

lazy val interpreter = project.in(file("modules/interpreter")).settings(
  scalaVersion := "2.12.6",
  libraryDependencies += "org.scalatest" %% "scalatest" % "3.0.5" % "test"
)
