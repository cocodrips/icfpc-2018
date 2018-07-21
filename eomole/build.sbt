lazy val root = project.settings(
  name := "icfpc2018",
  version := "0.1",
  scalaVersion := "2.12.6"
).aggregate(ai, js, bin)

lazy val ai = project.in(file("modules/ai")).settings(
  scalaVersion := "2.12.6"
).dependsOn(interpreter)

lazy val bin = project.in(file("modules/bin")).enablePlugins(JavaAppPackaging, UniversalPlugin).settings(
  name := "eomole-score",
  scalaVersion := "2.12.6"
).dependsOn(interpreter)

lazy val js = project.in(file("modules/js")).enablePlugins(ScalaJSPlugin).settings(
  scalaVersion := "2.12.6",
  scalaJSStage := FullOptStage
).dependsOn(interpreter)

lazy val interpreter = project.in(file("modules/interpreter")).settings(
  scalaVersion := "2.12.6",
  libraryDependencies += "org.scalatest" %% "scalatest" % "3.0.5" % "test"
)
