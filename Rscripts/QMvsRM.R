#SCHEDNAMES <- c("Rolling Machine Assignment", "Random", "GreedyLoadBalancing")
INSTANCES <- c(#"Pinedo",
               "30j-15r-4m.ms",
               "30j-15r-8m.ms",
               "30j-30r-4m.ms",
               "30j-30r-8m.ms",
               "30j-75r-4m.ms",
               "30j-75r-4m.ms",
               "100j-50r-6m.ms",
               "100j-50r-12m.ms",
               "100j-100r-6m.ms",
              "100j-100r-12m.ms",
              "100j-250r-6m.ms",
               "100j-250r-12m.ms")

#RM.ID <- 1
#QM.ID <- 2

###
########################## Libraries #########################################
###
installNeededPackages <- function()
{
  install.packages("ggplot2")
  install.packages("rlang")
  install.packages("plyr")
  install.packages("dplyr")
  install.packages("reshape")
  install.packages("xtable")
  library(ggplot2)
  library(plyr)
  library(dplyr)
  library(reshape)
  library(rlang)
  show("If no warnings shown, installation successfull.")
}

library(ggplot2)
library(plyr)
library(dplyr)
library(reshape)
library(rlang)
library(cowplot)
library(xtable)


###
########################## FUNCTIONS #########################################
###
GetPathToOutput <- function(InstanceName,ScheduleName,Nruns,FileType){
	# BasePath <- "C:\\Users\\Gebruiker\\Documents\\UU\\MSc Thesis\\Code\\OutPut\\" #laptop
  BasePath <- "C:\\Users\\3496724\\Source\\Repos\\SimulationTools\\Results\\RMs\\" #UU pc
	PathComponents <- c(BasePath,
				  "Instance_", InstanceName,
				  "_Schedule_", ScheduleName,
				  "_Runs_", Nruns,
				  "_",FileType,
				  ".txt")
	FilePath <- paste(PathComponents, collapse="")
	return(FilePath)
}

SKIPTHISSHIT <- function()
{
  if(FALSE)
  {GetSimSettings <- function(InstanceName,ScheduleName,Nruns){
    path <- GetPathToOutput(InstanceName,ScheduleName,Nruns,"SimSettings")
    return(read.csv2(file = path,header = FALSE))
  }
  GetQMs <- function(InstanceName,ScheduleName,Nruns){
    path <- GetPathToOutput(InstanceName,ScheduleName,Nruns,"QMs")
    return(read.csv2(file = path, header = FALSE))
  }
  
  GetInstanceName <- function(){
    return(SimSettings[1,])
  }
  
  GetScheduleName <- function(){
    return(SimSettings[2,])
  }
  
  GetRMName <- function(){
    f <- levels(GetSimSettings(INSTANCE,SCHEDNAMES[1],NRUNS)[2+2*RM.ID,])[6]
    return(f)
  }
  
  QMNames <- c("Cmax","Linear Start Delay","Start Punctuality","TODO: Completion Punctuality")
  GetQMName <- function(){
    return(QMNames[QM.ID])
  }
  
  GetRM <- function(SimSettings){
    f <- SimSettings[3+2*RM.ID,]
    return( as.numeric(levels(f))[f] )
  }
  
  GetQM <- function(QMData){
    return(QMData[,1+QM.ID])
  }
  
  
  
  GetCmaxColumn <- function(QMData){
    return(QMData[,2])
  }
  
  GetLinearDelayColumn <- function(){
    return(QMData[,3])
  }
  
  GetStartPunctualityColumn <- function(){
    return(QMData[,4])
  }
  
  GetCompletionPunctualiyColumn <- function(){
    return(QMData[,5])
  }
  BuildDfFor <- function(InstanceName,ScheduleName,Nruns){ 
    rmvals <- GetRM(GetSimSettings(InstanceName,ScheduleName,Nruns))
    cmax <- GetQM(GetQMs(InstanceName,ScheduleName,Nruns))
    df <- data.frame( RM = rmvals, Cmax = cmax)
    df.summary <- df %>% group_by(RM) %>% summarize(ymin = mean(cmax) - sd(cmax), ymax = mean(cmax) + sd(cmax),ymean = mean(cmax))
    df.summary$PointLabel <- abbreviate(ScheduleName)
    return( df.summary )
  }
  
  # build data frame:
  BuildPlot <- function(cScheduleNames,InstanceName,Nruns)
  {
    
    
    ##
    
    dflist <- lapply(cScheduleNames,FUN = BuildDfFor, InstanceName = InstanceName, Nruns = Nruns)
    dfplotinfo <- rbind.fill(dflist)
    
    return (dfplotinfo)
  }
  
  
    
    
  }
}
####





MakeQuantilePlot <-  function(string.RM,double.upperQ,type)
{
  if(type == "absolute")
  {
    p <- MakeAbsoluteQuantileDifPlot(string.RM,double.upperQ)
  }
  else if(type == "relative")
  {
    p <- MakeRelativeQuantileDifPlot(string.RM,double.upperQ)
  }
  else
  {
    show("Type parameter unrecognized. Must be absolute or relative")
    stop()
  }
  
  fileName <- paste(PATH,"_",Sys.Date(),"_",string.RM,"_vs_",type,"_",double.upperQ,"_quantile",".pdf",sep="")
  show(paste("Saving to:",fileName))
  ggsave(filename = fileName,plot = p)
  
}

MakeAbsoluteQuantileDifPlot <- function(string.RM,double.upperQ)
{
  library(dplyr)
  RMsym <- rlang::sym(string.RM)
  string.QM <- "Cmax"
  QMsym <- rlang::sym(string.QM)
  string.RelativeTo <- "DetCmax"
  RELsym <- rlang::sym(string.RelativeTo)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType) %>% 
    summarize(RM = mean(!!RMsym),QM = quantile((!!QMsym),double.upperQ)-mean(!!RELsym))
  
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")
  
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Instance.Name,shape=Distribution.Type)) 
  p <- p + geom_point() 
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(paste(100*double.upperQ,"% quantile Cmax - mean(Cmax)"))
  p <- p + ggtitle(paste(100*double.upperQ,"% quantile - 50% quantile. Spearman = ",Srho$estimate))
  show(p)
  
}

MakeRelativeQuantileDifPlot <- function(string.RM,double.upperQ)
{
  library(dplyr)
  RMsym <- rlang::sym(string.RM)
  string.QM <- "Cmax"
  QMsym <- rlang::sym(string.QM)
  string.RelativeTo <- "DetCmax"
  RELsym <- rlang::sym(string.RelativeTo)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType) %>% 
    summarize(RM = mean(!!RMsym),QM = (quantile((!!QMsym),double.upperQ)/mean(!!RELsym))-1)
  
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")
  
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Instance.Name)) 
  #p <- p + geom_text(aes(label=Instance.Name),hjust=0, vjust=0)
  p <- p + geom_point() 
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(paste(100*double.upperQ,"% quantile Cmax / DetCmax - 1"))
  p <- p + theme(legend.position = "top") + ggtitle(paste(100*double.upperQ,"% quantile / 50% quantile. Spearman = ",Srho$estimate))
  show(p)
  
}

MakeRelativeSDPlot <- function(string.RM)
{
  library(dplyr)
  RMsym <- rlang::sym(string.RM)
  string.QM <- "Cmax"
  QMsym <- rlang::sym(string.QM)
  string.RelativeTo <- "DetCmax"
  RELsym <- rlang::sym(string.RelativeTo)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QM = (sd(!!QMsym)/mean(!!RELsym)))
  
  View(myDF.plot)
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")
  
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.AssignType,shape=Schedule.StartTimeType,label=Instance.Name)) 
  #p <- p + geom_text(aes(label=Instance.Name),hjust=0, vjust=0)
  p <- p + geom_point() 
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab("sd(Cmax)/Cmax")
  p <- p + theme(legend.position = "top") + ggtitle(paste("sd(Cmax)/Cmax. Spearman = ",Srho$estimate))
  show(p)
  
}


MakePlot <- function(string.RM,string.QM,bool.IncludeXYline,param.plottype,DFin = myDF)
{
  library(ggplot2)
  RMsym <- sym(string.RM)
  QMsym <- sym(string.QM)
  
  if(param.plottype == "Distribution"){myDF.plot <- DFin %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QMsd = sd(!!QMsym),QM=mean(!!QMsym))
  
    show(myDF.plot)
  }
  else if(param.plottype == "95perc")
  {
    myDF.plot <- DFin %>% 
      group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
      summarize(RM = mean(!!RMsym),QM = quantile((!!QMsym),0.95))
  }
  else if(param.plottype =="Relative")
  {
    myDF.plot <- DFin %>% 
      group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
      summarize(RM = mean(!!RMsym),QM = sd(!!QMsym)/mean(!!QMsym))
    
  }
  else
  {
    show(paste("ERROR: param.plottype -",param.plottype,"- not recognized. Should be 95perc, Relative, or Distribution."))
    stop()
  }
  
  # names(myDF.plot)[3] <- string.RM
  #  names(myDF.plot)[4] <- paste(string.QM,"sd",sep="")
  #  names(myDF.plot)[5] <- string.QM
  Srho <- c(1:(length(INSTANCES)+1))
  index <- 1
  for(InsName in INSTANCES)
  {
    #show("All Instance names in myDF.plot:")
    #show(myDF.plot$Instance.Name)
    Xvals <- subset(myDF.plot,as.character(myDF.plot$Instance.Name)==InsName)[,"RM"][[1]]
    if(length(Xvals)<1)
    {
      #Instance was not simulated, ignore it
      show(paste("No results for instance:", InsName,"ignoring..."))
      
    }
    else
    {
      Yvals <- subset(myDF.plot,as.character(myDF.plot$Instance.Name)==InsName)[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
      Srho[[index]] <- cor.test(x=Xvals,y=Yvals,method = "spearman")[[4]]
      index <- index + 1
    }
    Srho[[length(INSTANCES)+1]] <- mean(Srho[1:length(INSTANCES)])
  }
  
  #Xvals <- myDF.plot[,"RM"][[1]]
  #Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  #Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")

  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Instance.Name,shape=Distribution.Type)) 
  p <- p + geom_point()
  if(param.plottype == "Distribution")
  {
    p <- p + geom_errorbar(aes(ymin=myDF.plot$QM-myDF.plot$QMsd,ymax=myDF.plot$QM+myDF.plot$QMsd))
    p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
    p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.02*max(myDF.plot$QM + myDF.plot$QMsd)))
  }
  else if(param.plottype == "95perc")
  {
    p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
    p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  }
  else if(param.plottype =="Relative")
  {
    p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
    p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  }
    
#  p <- p + theme(legend.position = "top") 
  
  if(param.plottype == "Distribution")
  {
    p <- p + ggtitle(paste(RMsym,"vs",QMsym,DISTRIBUTION,"jobs"))
    p <- p + xlab(string.RM) + ylab(string.QM)
  }
  else if(param.plottype == "95perc")
  {
    p <- p + ggtitle(paste(RMsym,"vs",QMsym,"95th q.",DISTRIBUTION,"jobs"))
    p <- p + xlab(string.RM) + ylab(paste(string.QM,"95th quantile"))
  }
  else if(param.plottype =="Relative")
  {
    p <- p + ggtitle(paste(RMsym,"vs",QMsym,"VarCo",DISTRIBUTION,"jobs"))
    p <- p + xlab(string.RM) + ylab(paste(string.QM,"sd/mean"))
  }
  
  if(bool.IncludeXYline == TRUE)
  {
    p <- p + geom_abline(slope=1,intercept = 0)
  }
  p<- p + theme_gray()
  p<- p+theme(text = element_text(size = 20))
  p <- p+guides(shape=FALSE)
  
  pfileName <- paste(PATH,"_",Sys.Date(),"_",DISTRIBUTION,string.RM,"_vs_",string.QM,sep ="")
  pfileName <- paste(pfileName,param.plottype,sep="_")
  pfileName <- paste(pfileName,"_PLOT.pdf",sep="")
  show(pfileName)
  ggsave(filename = pfileName,plot = p)
  
  if(TRUE)
  {
    
    library(gridExtra)
   # SummaryTable <- data.frame(INSTANCES,Srho)
  #  names(SummaryTable) <- c("Problem Instance", 
  #                          "Spearman Correlation")
    # Set theme to allow for plotmath expressions
   # tt <- ttheme_default(colhead=list(fg_params = list(parse=TRUE)))
  #  tbl <- tableGrob(SummaryTable, rows=NULL, theme=tt)
    # Plot chart and table into one object
   # g <- grid.arrange(p, tbl,
    #                  nrow=2,
    #                  as.table=TRUE,
    #                  heights=c(3,1))
    
    #gfileName <- paste(PATH,"_",Sys.Date(),"_",string.RM,"_vs_",string.QM,"_PLOT_withTABLE.pdf",sep="")
    #ggsave(filename = gfileName,plot = g)
    
    dfColumn <- data.frame(Srho)
    names(dfColumn) <- TexifyRM(string.RM)
    return(dfColumn)
    #return(p)
    
    
    
  }
  
}

Make.PLOTS <- function(PLOTS.list)
{
  table.df <- data.frame(c(INSTANCES,"Mean"))
  names(table.df) <- "Problem Instance"
  for(PlotArgs in PLOTS.list)
  {
    dfCol <- MakePlot(string.RM =PlotArgs[1],string.QM = PlotArgs[2],PlotArgs[3],param.plottype = PlotArgs[4])
    table.df <- cbind(table.df,dfCol)
  }
  return(table.df)
}

MakePlot.WithRange <- function(string.RM, string.QM,xRange,yRange)
{
  RMsym <- sym(string.RM)
  QMsym <- sym(string.QM)
  #RMsym <- sym("FS")
  #QMsym <- sym("Cmax")
  library(ggplot2)
  #library(tidyverse)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QMsd = sd(!!QMsym),QM=mean(!!QMsym))
  
  # names(myDF.plot)[3] <- string.RM
  #  names(myDF.plot)[4] <- paste(string.QM,"sd",sep="")
  #  names(myDF.plot)[5] <- string.QM
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Instance.Name,shape=Schedule.AssignType)) 
  p <- p + geom_point() 
  p <- p + geom_errorbar(aes(ymin=QM-QMsd,ymax=QM+QMsd))
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,xRange)) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,yRange))
  p <- p + xlab(paste("mean ",string.RM)) + ylab(string.QM)
  p <- p + theme(legend.position = "top")
  
  fileName <- paste(PATH,"_",Sys.Date(),"_",string.RM,"_vs_",string.QM,".pdf",sep="")
  ggsave(filename = fileName,plot = p)
  
  print(p)
  
}

MakeAllPlots <- function()
{
  RMs <- c("FS","wFS","UFS","TS","wTS","BTS","UTS","DetCmax","NormalApproxCmax","NormalApprox2sigma")
  QMs <- c("Cmax","LinearStartDelay","Start.Punctuality","Finish.Punctuality","DetCmax")
  
  for(Rm in RMs)
  {
    for(Qm in QMs)
    {
      MakePlot(Rm,Qm,FALSE,FALSE)
      MakePlot(Rm,Qm,TRUE,FALSE)
      MakePlot(Rm,Qm,FALSE,TRUE)
      MakePlot(Rm,Qm,TRUE,TRUE)
    }
    
    MakeQuantilePlot(Rm,0.95,"absolute")
    MakeQuantilePlot(Rm,0.95,"relative")
    
  }
  
}

Make.Different.PI.and.Distros.Plot <- function()
{
  myDF$PI <- myDF$Instance.Name
  PILabels <- c("B4_Diamond",
                "B4_FourCycles",
                "B4_FullDependency",
                "B4_NoInterMachine",
                "B4_RollingDiamond",
                "B4_SingleCycle",
                "B40_Diamond",
                "B40_FourCycles",
                "B40_FullDependency",
                "B40_NoInterMachine",
                "B40_RollingDiamond",
                "B40_SingleCycle")
  myDF$Cmax.Minus.NormalApprox <- myDF$Cmax - myDF$NormalApproxCmax;
  p <-ggplot(aes(y = Cmax.Minus.NormalApprox, x = Distribution.Type, fill = PI), data = myDF) + geom_boxplot() + theme(axis.text = element_text(size = 18), axis.title = element_text(size=15),legend.position = "top") + scale_fill_discrete(labels = PILabels)
  p <- p+ xlab("Distribution") + ylab("Realized Makespan - NormalApprox") + ggtitle("Realized Makespan - Normal Approximation for N(p,0.3p).")
  show(p)
  }

Texify <- function(myTab,DISTRIBUTION,QM.name)
{
  RMnames <- names(myTab)[-1]
  RMdescriptions <- paste(TexShortToTexLong(RMnames),collapse=", ")
  if(DISTRIBUTION == "Exponential")
  {
    xdist <- "Exp"
    texdist <- "$Exp(p)$"
  }
  else if(DISTRIBUTION == "Normal30")
  {
   xdist <- "N30"
   texdist <- "$N(p,0.3p)$"
  }
  else
  {
       show("Distribution type unkown")
       stop()
  }
  if(!(QM.name == "ECmax" ||QM.name == "SDoM" ||QM.name == "LSD" ||QM.name == "POOTJ" ||QM.name == "C95"))
  {
    show("QM name unkown")
    stop()
  }
  xlabel <- paste("tab:SC:",QM.name,":",xdist,sep="")
  xcaption <- paste("Spearman Correlation between  $\\",
                    QM.name,
                    "$ and ",
                    RMdescriptions,
                    ". Using ",texdist," jobs, 100 Schedules and 300 simulations per schedule.",sep="")
  print(xtable(myTab,label = xlabel, caption = xcaption),include.rownames = FALSE,sanitize.colnames.function = identity)
}

TexifyRM <- function(RM,longform = FALSE)
{
  texcommand <- ""
  if(!longform){texcommand <- "$"}
  if(RM == "FS"|| RM == "wFS"||
     RM == "BFS"|| RM == "wBFS"||
     RM == "UFS"|| RM == "wUFS"||
     RM == "TS"|| RM == "wTS"|| 
     RM == "BTS"|| RM == "wBTS"||
     RM == "UTS"||RM == "wUTS"||
     RM == "SDR"|| 
     RM == "DetCmax" || RM == "NormalApproxCmax")
  {
    texcommand <- paste(texcommand,"\\",RM,sep="")
  }
  else if(RM == "NormalApprox2Sigma")
  {
    texcommand <- paste(texcommand,"\\NormalApproxTwoSigma")
  }
  else
  {
    show(paste("ERROR in TexifyRM, RM:",RM," not recongnized"))
    stop()
  }
  if(longform)
  {
    texcommand <- paste(texcommand,"long ")
  }
  else
  {
    texcommand <- paste(texcommand,"$")
  }
  return(texcommand)
}

TexShortToTexLong <- function(shorttex)
{
  ncharshort <- nchar(shorttex)
  longtex <- substring(shorttex,3, ncharshort - 2)
  longtex <- paste("\\",longtex,"long{}",sep="")
  return(longtex)
  
}

LSDlist <- function(runID)
{
  IN <- "30j-15r-4m.ms"
  SA <- paste("RandomMLSID",runID,sep="")
  
  return(as.vector(
                  subset(myDF, 
                        Instance.Name == IN & Schedule.AssignType == SA,
                        select = c(LinearStartDelay)
                      )
                  )
  )
  
}

colSD <- function(df.in)
{
  apply(df.in,2,sd)
}
#
#
######## ACTUAL WORK ##########################################
#
#


#plot.df <- BuildPlot(SCHEDNAMES,INSTANCE,NRUNS)
#errorBarWidth <- max(plot.df$RM) / (4 * length(unique(plot.df$RM)))
#p <- ggplot(plot.df, aes(x = RM, y = ymean, label= PointLabel)) + geom_point(size = 2) + geom_errorbar(aes(ymin = ymin, ymax = ymax, width = errorBarWidth))+geom_text(aes(label=PointLabel),hjust=0, vjust=0)
#p + labs(x = paste(c("Schedule ",abbreviate(GetRMName())," score"),collapse=''), y = GetQMName())

########### plot from one big data file ############


UUPATH <- "C:/Users/3496724/Source/Repos/SimulationTools/Results/RMs/allresults.txt"
DISTRIBUTION <- "Normal30"
GetPathEnding <- function(Distribution.Enum)
{
  if(Distribution.Enum == "Normal30")
  {
    return("FinalResults2ResultHarderN30.txt")
    
  }
  else if(Distribution.Enum =="Exponential")
  {
    return("FinalResults2ResultHarderExp.txt")
  }
  else
  {
    show("ERROROROROROROROROR PROBLEM PROBLEM!!! Distro type not recognized.")
    stop()
  }
}
LAPTOPPATH <- paste("C:/Users/Gebruiker/Documents/UU/MSc Thesis/Code/Simulation/SimulationTools/Results/RMs/",GetPathEnding(DISTRIBUTION),sep="")
PATH <- LAPTOPPATH
myDF <- read.csv2(LAPTOPPATH)
NRUNS <- "300"

#### TABLE MAKING
ECmaxTable <-Make.PLOTS(ECmaxPLOTS)
Texify(ECmaxTable,DISTRIBUTION,"ECmax")

POOTJTable <- Make.PLOTS(StartPunctualityPLOTS)
Texify(POOTJTable,DISTRIBUTION,"POOTJ")

LSDTable <- Make.PLOTS(LinearStartDelayPLOTS)
Texify(LSDTable,DISTRIBUTION,"LSD")

C95Table <- Make.PLOTS(QualityRobustness95PLOTS)
Texify(C95Table,DISTRIBUTION,"C95")

SDoMTable <- Make.PLOTS(QualityRobustnessRelativePlots)
Texify(SDoMTable,DISTRIBUTION,"SDoM")

p <- MakePlot(string.RM = "DetCmax",string.QM = "NormalApproxCmax",TRUE,FALSE)

Spearmans.Table <- Make.All.UsefullPlots()

MakePlot.WithRange("DetCmax","Cmax",500,500)
MakeAllPlots()
MakeQuantilePlot("BTS",0.95,type="relative")
Make.Different.PI.and.Distros.Plot()


####usefull plots:
#RM,QM,PrintABline,Use quantile

#list: RM,QM,Abline,Percentile
ECmaxPLOTS <- list(
  c("DetCmax","Cmax",TRUE,"Distribution"),                 #E(Cmax)
  c("NormalApproxCmax","Cmax",TRUE,"Distribution"),
  c("FS","Cmax",TRUE,"Distribution")
  #c("TS","Cmax",TRUE,"Distribution")
)

StartPunctualityPLOTS <- list(
  #c("DetCmax","Start.Punctuality",FALSE,"Distribution"),   #Solution Robustness
  c("FS","Start.Punctuality",FALSE,"Distribution")
 # c("BFS","Start.Punctuality",FALSE,"Distribution")
  #c("UFS","Start.Punctuality",FALSE,"Distribution"),
  #c("wFS","Start.Punctuality",FALSE,"Distribution"),
  #c("wBFS","Start.Punctuality",FALSE,FALSE),
  #c("wUFS","Start.Punctuality",FALSE,"Distribution")
)
LinearStartDelayPLOTS<- list(
#  c("DetCmax","LinearStartDelay",FALSE,"Distribution"),
  #c("FS","LinearStartDelay",FALSE,"Distribution"),
  c("BFS","LinearStartDelay",FALSE,"Distribution"),
#  c("UFS","LinearStartDelay",FALSE,"Distribution"),
#  c("wFS","LinearStartDelay",FALSE,"Distribution"),
 # c("wBFS","LinearStartDelay",FALSE,FALSE),
  c("wUFS","LinearStartDelay",FALSE,"Distribution")
)

QualityRobustness95PLOTS <- list(
  c("DetCmax","Cmax",FALSE,"95perc"),                 #Quality Robustness (all vs 95%)
  c("NormalApproxCmax","Cmax",FALSE,"95perc")
)

  c("NormalApprox2Sigma","Cmax",TRUE,"95perc"),
  c("TS","Cmax",FALSE,"95perc"),
  c("BTS","Cmax",FALSE,"95perc"),
  c("UTS","Cmax",FALSE,"95perc"),
  c("wTS","Cmax",FALSE,"95perc"),
  #c("wBTS","Cmax",FALSE,TRUE),
  #c("wUTS","Cmax",FALSE,TRUE),
  c("SDR","Cmax",FALSE,"95perc")
)

QualityRobustnessRelativePlots <- list(
 # c("DetCmax","Cmax",FALSE,"Relative"),                 #Quality Robustness (all vs 95%)
  c("NormalApproxCmax","Cmax",FALSE,"Relative"))
  c("NormalApprox2Sigma","Cmax",TRUE,"Relative"),
  c("TS","Cmax",FALSE,"Relative"),
  c("BTS","Cmax",FALSE,"Relative"),
  c("UTS","Cmax",FALSE,"Relative"),
  c("wTS","Cmax",FALSE,"Relative"),
  #c("wBTS","Cmax",FALSE,TRUE),
  #c("wUTS","Cmax",FALSE,TRUE),
  c("SDR","Cmax",FALSE,"Relative")
)

All.Usefull.PLOTS <- list(
  
)


Make.All.UsefullPlots()
############################
#For debugging
myDF.plot <- myDF %>% 
  group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
  summarize(SS1=mean(ScheduledStartTime1),Delay=mean(RealisedStartTime1-ScheduledStartTime1))

PlotSchedStartvsDelay <- function(string.Instance,string.AssignType)
{
  library(reshape)
  myDF <- read.csv2(PATH)
  myDF <- subset(myDF,grepl(string.Instance,Instance.Name))
  myDF <- subset(myDF,grepl(string.AssignType,Schedule.AssignType))
  melted.myDF <- melt(myDF,id=names(myDF)[1:12])
  my.melted.DF.plot <- melted.myDF %>% 
    group_by(Instance.Name,Schedule.AssignType,Schedule.StartTimeType,variable) %>% 
    summarize(mval = mean(value),sdval = sd(value))
  View(my.melted.DF.plot)
  melted2.df.plot <- subset(my.melted.DF.plot,grepl("Scheduled", variable, fixed=TRUE))
  xval <- subset(my.melted.DF.plot,grepl("Scheduled", variable, fixed=TRUE))$mval
  yval <- subset(my.melted.DF.plot,grepl("RealisedStartTime", variable, fixed=TRUE))$mval - xval
  yvalsd <- subset(my.melted.DF.plot,grepl("RealisedStartTime", variable, fixed=TRUE))$sdval
  df.plot <- data.frame(melted2.df.plot,yval,yvalsd)
  p <- ggplot(df.plot,aes(x=mval,y=yval,group=interaction(Schedule.AssignType,Instance.Name),colour=Schedule.AssignType,shape=Instance.Name))
  p + geom_point()
  p <- p + geom_errorbar(aes(ymin=yval-yvalsd,ymax=yval+yvalsd))
  p <- p + xlab(paste("Jobstarttime")) + ylab("Delay")
  p <- p + theme(legend.position = "top")
  show(p)
}
   


starttimes <- subset(my.melted.DF.plot,grepl("Scheduled",variable,fixed = TRUE))$mval
cbind(my.melted.DF.plot,starttimes)

ggplot(myDF.plot,aes(x=SS1,y=Delay,colour = Schedule.AssignType,shape=Schedule.AssignType)) + geom_point()

p <- ggplot(myDF.plot,aes(x=FS,y=Cmax,colour=Schedule.StartTimeType,shape=Schedule.StartTimeType)) 
p <- p + geom_point() 
p <- p + geom_errorbar(aes(ymin=Cmax-Cmaxsd,ymax=Cmax+Cmaxsd))
p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,2000)) 
p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,500))

p

############ Sampling tests

Zs <- rnorm(10000)
mean(Zs)
LNmean <- 5
LNvar <- 10^2
mu <- log(LNmean^2 / sqrt(LNvar + LNmean^2))
sigma <- sqrt(log(1+LNvar/LNmean^2))
Xs <- exp(mu + sigma*Zs)
mean(Xs)
sd(Xs)

#MakeAllPlots()

PI <- c("30j-15r-8m.ms",
        "30j-30r-4m.ms",
        "30j-30r-8m.ms",
        "100j-50r-6m.ms",
        "100j-50r-12m.ms",
        "100j-100r-6m.ms",
        "100j-100r-12m.ms")

