// component on entities that provides jobs
// sibling of Location
public partial class JobProvider {
	public List<Job> jobs;
}

public struct LaborRequirement {
	// required pop profession
	public DefRef<PopProfessionType> profession { get; set; }
	// number of pops required
	public int amount { get; set; }
}

public enum JobStatus {
	Inactive,
	Active,
	Finished,
}

public enum JobType {
	Construction,
	Production,
	Extraction,
}

public abstract class Job {
	public JobStatus status = JobStatus.Inactive;
	public bool isRepeating = false;
	public List<LaborRequirement> laborRequirement;
}

public partial class ConstructionJob : Job {
	public Entity constructionSite;
}

// component on entities that can perform jobs
public partial class HasJob {
	public Job job;
}